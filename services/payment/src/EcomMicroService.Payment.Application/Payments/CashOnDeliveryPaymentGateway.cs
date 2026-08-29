using System.Threading.Tasks;

namespace EcomMicroService.Payment;

/// <summary>
/// Cash on delivery: no online payment; customer pays when the order is delivered.
/// </summary>
public class CashOnDeliveryPaymentGateway : IPaymentGateway
{
    public const string GatewayName = "CashOnDelivery";

    public string Name => GatewayName;
    public string? PublishableKeyOrClientId => null;

    public Task<CreatePaymentIntentResult> CreatePaymentIntentAsync(CreatePaymentIntentRequest request)
    {
        return Task.FromResult(new CreatePaymentIntentResult
        {
            Success = true,
            GatewayPaymentId = "COD",
            ClientSecret = null,
            PublishableKeyOrClientId = null
        });
    }

    public Task<ConfirmPaymentResult> ConfirmPaymentAsync(ConfirmPaymentRequest request)
    {
        return Task.FromResult(new ConfirmPaymentResult { Success = true });
    }

    public Task<RefundPaymentResult> RefundPaymentAsync(RefundPaymentRequest request)
    {
        return Task.FromResult(new RefundPaymentResult
        {
            Success = false,
            ErrorCode = "NotApplicable",
            ErrorMessage = "Cash on delivery orders are not refunded through the payment module."
        });
    }
}

