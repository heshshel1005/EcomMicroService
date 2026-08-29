using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace EcomMicroService.Payment;

[Volo.Abp.RemoteService(IsEnabled = false)]
public interface IPaymentAppService : IApplicationService
{
    Task<List<PaymentGatewayDto>> GetGatewaysAsync();
    Task<CreatePaymentIntentResult> CreatePaymentIntentAsync(Guid orderId, string gatewayName);
    Task<ConfirmPaymentResult> ConfirmPaymentAsync(Guid orderId, string gatewayPaymentId);
    Task<RefundPaymentResult> RefundAsync(Guid orderId, decimal? amount = null, string? reason = null);
    Task<PaymentResultDto> PayCashOnDeliveryAsync(Guid orderId);
}

public class PaymentResultDto
{
    public Guid OrderId { get; set; }
    public string Gateway { get; set; } = "COD";
    public string Status { get; set; } = "Pending";
}

public interface IPaymentGateway
{
    string Name { get; }
    string? PublishableKeyOrClientId { get; }
    Task<CreatePaymentIntentResult> CreatePaymentIntentAsync(CreatePaymentIntentRequest request);
    Task<ConfirmPaymentResult> ConfirmPaymentAsync(ConfirmPaymentRequest request);
    Task<RefundPaymentResult> RefundPaymentAsync(RefundPaymentRequest request);
}

public class PaymentGatewayDto
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? PublishableKeyOrClientId { get; set; }
}

public class CreatePaymentIntentRequest
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
    public string? CustomerEmail { get; set; }
    public string? Description { get; set; }
}

public class CreatePaymentIntentResult
{
    public bool Success { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ClientSecret { get; set; }
    public string? GatewayPaymentId { get; set; }
    public string? PublishableKeyOrClientId { get; set; }
}

public class ConfirmPaymentRequest
{
    public Guid OrderId { get; set; }
    public string GatewayPaymentId { get; set; } = string.Empty;
}

public class ConfirmPaymentResult
{
    public bool Success { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

public class RefundPaymentRequest
{
    public Guid OrderId { get; set; }
    public string? GatewayPaymentId { get; set; }
    public decimal? Amount { get; set; }
    public string? Reason { get; set; }
}

public class RefundPaymentResult
{
    public bool Success { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}
