using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace EcomMicroService.Payment;

/// <summary>
/// Paymob Accept payment gateway (iframe). Used in MENA. No raw card data is stored.
/// Flow: auth token → register order → payment key → frontend shows iframe; callback/redirect sends transaction id for confirm.
/// </summary>
public class PaymobPaymentGateway : IPaymentGateway
{
    public const string GatewayName = "Paymob";
    private const string BaseUrl = "https://accept.paymobsolutions.com/api";

    private readonly PaymobPaymentGatewayOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;

    public string Name => GatewayName;
    public string? PublishableKeyOrClientId => _options.IframeId > 0 ? _options.IframeId.ToString() : null;

    public PaymobPaymentGateway(IOptions<PaymobPaymentGatewayOptions> options, IHttpClientFactory httpClientFactory)
    {
        _options = options?.Value ?? new PaymobPaymentGatewayOptions();
        _httpClientFactory = httpClientFactory;
    }

    private async Task<string?> GetAuthTokenAsync()
    {
        if (string.IsNullOrEmpty(_options.ApiKey))
            return null;
        var client = _httpClientFactory.CreateClient();
        var body = JsonSerializer.Serialize(new { api_key = _options.ApiKey });
        var req = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/auth/tokens")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        var response = await client.SendAsync(req);
        if (!response.IsSuccessStatusCode)
            return null;
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        return doc.RootElement.TryGetProperty("token", out var tok) ? tok.GetString() : null;
    }

    public async Task<CreatePaymentIntentResult> CreatePaymentIntentAsync(CreatePaymentIntentRequest request)
    {
        if (string.IsNullOrEmpty(_options.ApiKey) || _options.IntegrationId <= 0 || _options.IframeId <= 0)
            return new CreatePaymentIntentResult { Success = false, ErrorCode = "Config", ErrorMessage = "Paymob is not configured." };

        var token = await GetAuthTokenAsync();
        if (string.IsNullOrEmpty(token))
            return new CreatePaymentIntentResult { Success = false, ErrorCode = "Auth", ErrorMessage = "Failed to get Paymob auth token." };

        var client = _httpClientFactory.CreateClient();
        var amountCents = (long)Math.Round(request.Amount * 100);
        var currency = (request.Currency ?? "EGP").ToUpperInvariant();

        // 1. Register order
        var orderBody = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["auth_token"] = token,
            ["delivery_needed"] = "false",
            ["amount_cents"] = amountCents,
            ["currency"] = currency,
            ["merchant_order_id"] = request.OrderId.ToString()
        });
        var orderReq = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/ecommerce/orders")
        {
            Content = new StringContent(orderBody, Encoding.UTF8, "application/json")
        };
        var orderResponse = await client.SendAsync(orderReq);
        var orderJson = await orderResponse.Content.ReadAsStringAsync();
        if (!orderResponse.IsSuccessStatusCode)
            return new CreatePaymentIntentResult { Success = false, ErrorCode = "Order", ErrorMessage = orderJson };

        var orderDoc = JsonDocument.Parse(orderJson);
        if (!orderDoc.RootElement.TryGetProperty("id", out var orderIdProp))
            return new CreatePaymentIntentResult { Success = false, ErrorCode = "Order", ErrorMessage = "No order id in response." };
        var orderId = orderIdProp.GetInt64();

        // 2. Payment key
        var firstName = "Customer";
        var lastName = "";
        if (!string.IsNullOrEmpty(request.CustomerEmail))
        {
            var parts = request.CustomerEmail.Split('@')[0].Split(new[] { '.', '_' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2) { firstName = parts[0]; lastName = parts[1]; }
            else if (parts.Length == 1) firstName = parts[0];
        }
        var billingData = new Dictionary<string, string>
        {
            ["first_name"] = firstName,
            ["last_name"] = lastName,
            ["email"] = request.CustomerEmail ?? "customer@example.com",
            ["phone_number"] = "01000000000",
            ["apartment"] = "NA",
            ["floor"] = "NA",
            ["street"] = "NA",
            ["building"] = "NA",
            ["city"] = "NA",
            ["state"] = "NA",
            ["country"] = "EG"
        };
        var keyBody = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["auth_token"] = token,
            ["amount_cents"] = amountCents,
            ["currency"] = currency,
            ["order_id"] = orderId,
            ["integration_id"] = _options.IntegrationId,
            ["billing_data"] = billingData,
            ["lock_order_when_paid"] = "false"
        });
        var keyReq = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/acceptance/payment_keys")
        {
            Content = new StringContent(keyBody, Encoding.UTF8, "application/json")
        };
        var keyResponse = await client.SendAsync(keyReq);
        var keyJson = await keyResponse.Content.ReadAsStringAsync();
        if (!keyResponse.IsSuccessStatusCode)
            return new CreatePaymentIntentResult { Success = false, ErrorCode = "PaymentKey", ErrorMessage = keyJson };

        var keyDoc = JsonDocument.Parse(keyJson);
        var paymentToken = keyDoc.RootElement.TryGetProperty("token", out var t) ? t.GetString() : null;
        if (string.IsNullOrEmpty(paymentToken))
            return new CreatePaymentIntentResult { Success = false, ErrorCode = "PaymentKey", ErrorMessage = "No token in response." };

        return new CreatePaymentIntentResult
        {
            Success = true,
            ClientSecret = paymentToken,
            GatewayPaymentId = orderId.ToString(),
            PublishableKeyOrClientId = _options.IframeId.ToString()
        };
    }

    public async Task<ConfirmPaymentResult> ConfirmPaymentAsync(ConfirmPaymentRequest request)
    {
        if (string.IsNullOrEmpty(_options.ApiKey))
            return new ConfirmPaymentResult { Success = false, ErrorCode = "Config", ErrorMessage = "Paymob is not configured." };

        var token = await GetAuthTokenAsync();
        if (string.IsNullOrEmpty(token))
            return new ConfirmPaymentResult { Success = false, ErrorCode = "Auth", ErrorMessage = "Failed to get Paymob auth token." };

        var client = _httpClientFactory.CreateClient();
        var inquiryUrl = $"{BaseUrl}/acceptance/transactions/{request.GatewayPaymentId}?token={Uri.EscapeDataString(token)}";
        var req = new HttpRequestMessage(HttpMethod.Get, inquiryUrl);
        var response = await client.SendAsync(req);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            return new ConfirmPaymentResult { Success = false, ErrorCode = response.StatusCode.ToString(), ErrorMessage = body };

        try
        {
            var doc = JsonDocument.Parse(body);
            var success = doc.RootElement.TryGetProperty("success", out var s) && s.GetBoolean();
            if (success)
                return new ConfirmPaymentResult { Success = true };
            return new ConfirmPaymentResult { Success = false, ErrorCode = "NotSuccess", ErrorMessage = "Transaction not successful." };
        }
        catch
        {
            return new ConfirmPaymentResult { Success = false, ErrorCode = "Parse", ErrorMessage = body };
        }
    }

    public async Task<RefundPaymentResult> RefundPaymentAsync(RefundPaymentRequest request)
    {
        if (string.IsNullOrEmpty(request.GatewayPaymentId))
            return new RefundPaymentResult { Success = false, ErrorCode = "Invalid", ErrorMessage = "Gateway payment id is required for refund." };
        if (string.IsNullOrEmpty(_options.ApiKey))
            return new RefundPaymentResult { Success = false, ErrorCode = "Config", ErrorMessage = "Paymob is not configured." };

        var token = await GetAuthTokenAsync();
        if (string.IsNullOrEmpty(token))
            return new RefundPaymentResult { Success = false, ErrorCode = "Auth", ErrorMessage = "Failed to get Paymob auth token." };

        var client = _httpClientFactory.CreateClient();
        var payload = new Dictionary<string, object>
        {
            ["auth_token"] = token,
            ["transaction_id"] = request.GatewayPaymentId
        };
        if (request.Amount.HasValue && request.Amount.Value > 0)
            payload["amount_cents"] = (long)Math.Round(request.Amount.Value * 100);

        var body = JsonSerializer.Serialize(payload);
        var req = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/acceptance/void_refund/refund")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        var response = await client.SendAsync(req);
        var responseBody = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            return new RefundPaymentResult { Success = false, ErrorCode = response.StatusCode.ToString(), ErrorMessage = responseBody };
        return new RefundPaymentResult { Success = true };
    }
}

