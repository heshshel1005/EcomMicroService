namespace EcomMicroService.Payment;

/// <summary>
/// Paymob Accept API credentials. Used for iframe/card payments. No raw card data is stored.
/// </summary>
public class PaymobPaymentGatewayOptions
{
    public const string SectionName = "Paymob";
    /// <summary>API key from Paymob Accept dashboard.</summary>
    public string ApiKey { get; set; } = string.Empty;
    /// <summary>Iframe id for the payment form (from Paymob dashboard).</summary>
    public int IframeId { get; set; }
    /// <summary>Card integration id from Paymob dashboard.</summary>
    public int IntegrationId { get; set; }
    /// <summary>Optional: HMAC secret for callback verification.</summary>
    public string? HmacSecret { get; set; }
}

