using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace EcomMicroService.Payment;

public class ShopPaymentAppService : ApplicationService, IPaymentAppService
{
    private readonly IEnumerable<IPaymentGateway> _gateways;
    private readonly OrderingPaymentClient _ordering;

    public ShopPaymentAppService(IEnumerable<IPaymentGateway> gateways, OrderingPaymentClient ordering)
    {
        _gateways = gateways;
        _ordering = ordering;
    }

    [Authorize]
    public Task<List<PaymentGatewayDto>> GetGatewaysAsync()
    {
        var list = _gateways.Select(g => new PaymentGatewayDto
        {
            Name = g.Name,
            DisplayName = g.Name == CashOnDeliveryPaymentGateway.GatewayName ? "Cash on Delivery" : g.Name,
            PublishableKeyOrClientId = g.PublishableKeyOrClientId
        }).ToList();
        return Task.FromResult(list);
    }

    [Authorize]
    public async Task<CreatePaymentIntentResult> CreatePaymentIntentAsync(Guid orderId, string gatewayName)
    {
        var order = await _ordering.GetSnapshotAsync(orderId);
        if (string.Equals(order.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase))
            return new CreatePaymentIntentResult { Success = false, ErrorCode = "AlreadyPaid", ErrorMessage = "Order is already paid." };

        var gateway = GetGateway(gatewayName);
        return await gateway.CreatePaymentIntentAsync(new CreatePaymentIntentRequest
        {
            OrderId = orderId,
            Amount = order.Total,
            Currency = "usd",
            CustomerEmail = order.ContactEmail,
            Description = $"Order {orderId}"
        });
    }

    [Authorize]
    public async Task<ConfirmPaymentResult> ConfirmPaymentAsync(Guid orderId, string gatewayPaymentId)
    {
        var order = await _ordering.GetSnapshotAsync(orderId);
        var gatewayName = order.PaymentGateway ?? InferGatewayFromPaymentId(gatewayPaymentId);
        var gateway = GetGateway(gatewayName);
        var result = await gateway.ConfirmPaymentAsync(new ConfirmPaymentRequest { OrderId = orderId, GatewayPaymentId = gatewayPaymentId });
        if (!result.Success)
            return result;

        var isCod = gateway.Name == CashOnDeliveryPaymentGateway.GatewayName;
        await _ordering.ApplyPaymentAsync(orderId, gateway.Name, gatewayPaymentId, isCod ? "CashOnDelivery" : "Paid");
        return result;
    }

    [Authorize("ECommerce.Administration")]
    public async Task<RefundPaymentResult> RefundAsync(Guid orderId, decimal? amount = null, string? reason = null)
    {
        var order = await _ordering.GetSnapshotAsync(orderId);
        if (string.IsNullOrEmpty(order.PaymentGateway) || string.IsNullOrEmpty(order.ExternalPaymentId))
            return new RefundPaymentResult { Success = false, ErrorCode = "NoPayment", ErrorMessage = "Order has no payment to refund." };

        var gateway = GetGateway(order.PaymentGateway);
        return await gateway.RefundPaymentAsync(new RefundPaymentRequest
        {
            OrderId = orderId,
            GatewayPaymentId = order.ExternalPaymentId,
            Amount = amount,
            Reason = reason
        });
    }

    public Task<PaymentResultDto> PayCashOnDeliveryAsync(Guid orderId) =>
        ConfirmPaymentAsync(orderId, "COD").ContinueWith(t => new PaymentResultDto
        {
            OrderId = orderId,
            Gateway = "COD",
            Status = t.Result.Success ? "Pending" : "Failed"
        });

    private IPaymentGateway GetGateway(string name) =>
        _gateways.FirstOrDefault(g => string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase))
        ?? throw new Volo.Abp.BusinessException("ECommerce:UnknownPaymentGateway").WithData("Name", name);

    private static string InferGatewayFromPaymentId(string id)
    {
        if (string.Equals(id, "COD", StringComparison.OrdinalIgnoreCase)) return CashOnDeliveryPaymentGateway.GatewayName;
        return "PayPal";
    }
}

public class OrderingPaymentClient : ITransientDependency
{
    private readonly HttpClient _http;

    public OrderingPaymentClient(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _http = httpClientFactory.CreateClient("Ordering");
        var baseUrl = configuration["RemoteServices:Ordering:BaseUrl"] ?? "https://localhost:7007/";
        _http.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    }

    public async Task<OrderPaymentSnapshotDto> GetSnapshotAsync(Guid orderId)
    {
        return await _http.GetFromJsonAsync<OrderPaymentSnapshotDto>($"api/ordering/orders/{orderId}/payment-snapshot")
               ?? throw new Volo.Abp.BusinessException("ECommerce:OrderNotFound");
    }

    public async Task ApplyPaymentAsync(Guid orderId, string gateway, string externalPaymentId, string paymentStatus)
    {
        var response = await _http.PostAsJsonAsync($"api/ordering/orders/{orderId}/apply-payment", new
        {
            gateway,
            externalPaymentId,
            paymentStatus
        });
        response.EnsureSuccessStatusCode();
    }
}

public class OrderPaymentSnapshotDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string PaymentStatus { get; set; } = "None";
    public string? PaymentGateway { get; set; }
    public string? ExternalPaymentId { get; set; }
}
