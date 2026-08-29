namespace EcomMicroService.Payment;

/// <summary>
/// PayPal REST API credentials. Use sandbox for testing. No card data is stored by the app.
/// </summary>
public class PayPalPaymentGatewayOptions
{
    public const string SectionName = "PayPal";
    public string ClientId { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public bool UseSandbox { get; set; } = true;
}

