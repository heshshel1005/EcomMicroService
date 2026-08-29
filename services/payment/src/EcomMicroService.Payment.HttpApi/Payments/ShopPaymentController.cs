using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace EcomMicroService.Payment;

[RemoteService(Name = "Payment")]
[Area("payment")]
[Route("api/payment")]
[Authorize]
public class PaymentApiController : PaymentController
{
    private readonly IPaymentAppService _paymentAppService;

    public PaymentApiController(IPaymentAppService paymentAppService)
    {
        _paymentAppService = paymentAppService;
    }

    [HttpGet("gateways")]
    public Task<List<PaymentGatewayDto>> GetGatewaysAsync() => _paymentAppService.GetGatewaysAsync();

    [HttpPost("create-intent")]
    public Task<CreatePaymentIntentResult> CreatePaymentIntentAsync([FromBody] CreatePaymentIntentRequestDto input) =>
        _paymentAppService.CreatePaymentIntentAsync(input.OrderId, input.GatewayName);

    [HttpPost("confirm")]
    public Task<ConfirmPaymentResult> ConfirmPaymentAsync([FromBody] ConfirmPaymentRequestDto input) =>
        _paymentAppService.ConfirmPaymentAsync(input.OrderId, input.GatewayPaymentId);

    [HttpPost("refund")]
    [Authorize("ECommerce.Administration")]
    public Task<RefundPaymentResult> RefundAsync([FromBody] RefundPaymentRequestDto input) =>
        _paymentAppService.RefundAsync(input.OrderId, input.Amount, input.Reason);

    [HttpPost("cod/{orderId}")]
    public Task<PaymentResultDto> PayCashOnDeliveryAsync(Guid orderId) =>
        _paymentAppService.PayCashOnDeliveryAsync(orderId);
}

public class CreatePaymentIntentRequestDto
{
    public Guid OrderId { get; set; }
    public string GatewayName { get; set; } = string.Empty;
}

public class ConfirmPaymentRequestDto
{
    public Guid OrderId { get; set; }
    public string GatewayPaymentId { get; set; } = string.Empty;
}

public class RefundPaymentRequestDto
{
    public Guid OrderId { get; set; }
    public decimal? Amount { get; set; }
    public string? Reason { get; set; }
}
