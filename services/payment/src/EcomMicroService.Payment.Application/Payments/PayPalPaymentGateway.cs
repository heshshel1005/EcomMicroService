using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace EcomMicroService.Payment;

/// <summary>
/// PayPal payment gateway via REST API v2. No raw card data is stored.
/// </summary>
public class PayPalPaymentGateway : IPaymentGateway
{
    public const string GatewayName = "PayPal";
    private readonly PayPalPaymentGatewayOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public string Name => GatewayName;
    public string? PublishableKeyOrClientId => string.IsNullOrEmpty(_options.ClientId) ? null : _options.ClientId;

    public PayPalPaymentGateway(IOptions<PayPalPaymentGatewayOptions> options, IHttpClientFactory httpClientFactory)
    {
        _options = options?.Value ?? new PayPalPaymentGatewayOptions();
        _httpClientFactory = httpClientFactory;
    }

    private string BaseUrl => _options.UseSandbox
        ? "https://api-m.sandbox.paypal.com"
        : "https://api-m.paypal.com";

    private async Task<string?> GetAccessTokenAsync()
    {
        if (string.IsNullOrEmpty(_options.ClientId) || string.IsNullOrEmpty(_options.Secret))
            return null;
        var client = _httpClientFactory.CreateClient();
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.Secret}"));
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/v1/oauth2/token");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        request.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        return doc.RootElement.TryGetProperty("access_token", out var tok) ? tok.GetString() : null;
    }

    public async Task<CreatePaymentIntentResult> CreatePaymentIntentAsync(CreatePaymentIntentRequest request)
    {
        if (string.IsNullOrEmpty(_options.ClientId) || string.IsNullOrEmpty(_options.Secret))
            return new CreatePaymentIntentResult { Success = false, ErrorCode = "Config", ErrorMessage = "PayPal is not configured." };

        var token = await GetAccessTokenAsync();
        if (string.IsNullOrEmpty(token))
            return new CreatePaymentIntentResult { Success = false, ErrorCode = "Auth", ErrorMessage = "Failed to get PayPal access token." };

        try
        {
            var payload = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                    new
                    {
                        reference_id = request.OrderId.ToString(),
                        amount = new { currency_code = (request.Currency ?? "USD").ToUpperInvariant(), value = request.Amount.ToString("F2") },
                        description = request.Description
                    }
                }
            };
            var client = _httpClientFactory.CreateClient();
            var req = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/v2/checkout/orders");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await client.SendAsync(req);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return new CreatePaymentIntentResult
                {
                    Success = false,
                    ErrorCode = response.StatusCode.ToString(),
                    ErrorMessage = body
                };
            }
            var doc = JsonDocument.Parse(body);
            var id = doc.RootElement.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
            return new CreatePaymentIntentResult
            {
                Success = true,
                GatewayPaymentId = id,
                PublishableKeyOrClientId = _options.ClientId
            };
        }
        catch (Exception ex)
        {
            return new CreatePaymentIntentResult
            {
                Success = false,
                ErrorCode = "Exception",
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<ConfirmPaymentResult> ConfirmPaymentAsync(ConfirmPaymentRequest request)
    {
        if (string.IsNullOrEmpty(_options.ClientId) || string.IsNullOrEmpty(_options.Secret))
            return new ConfirmPaymentResult { Success = false, ErrorCode = "Config", ErrorMessage = "PayPal is not configured." };

        var token = await GetAccessTokenAsync();
        if (string.IsNullOrEmpty(token))
            return new ConfirmPaymentResult { Success = false, ErrorCode = "Auth", ErrorMessage = "Failed to get PayPal access token." };

        try
        {
            var client = _httpClientFactory.CreateClient();
            var req = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/v2/checkout/orders/{request.GatewayPaymentId}/capture");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            req.Content = new StringContent("{}", Encoding.UTF8, "application/json");
            var response = await client.SendAsync(req);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                return new ConfirmPaymentResult { Success = false, ErrorCode = response.StatusCode.ToString(), ErrorMessage = body };
            return new ConfirmPaymentResult { Success = true };
        }
        catch (Exception ex)
        {
            return new ConfirmPaymentResult { Success = false, ErrorCode = "Exception", ErrorMessage = ex.Message };
        }
    }

    public async Task<RefundPaymentResult> RefundPaymentAsync(RefundPaymentRequest request)
    {
        if (string.IsNullOrEmpty(request.GatewayPaymentId))
            return new RefundPaymentResult { Success = false, ErrorCode = "Invalid", ErrorMessage = "Gateway payment id is required for refund." };
        if (string.IsNullOrEmpty(_options.ClientId) || string.IsNullOrEmpty(_options.Secret))
            return new RefundPaymentResult { Success = false, ErrorCode = "Config", ErrorMessage = "PayPal is not configured." };

        var token = await GetAccessTokenAsync();
        if (string.IsNullOrEmpty(token))
            return new RefundPaymentResult { Success = false, ErrorCode = "Auth", ErrorMessage = "Failed to get PayPal access token." };

        // PayPal: get capture id from order then refund. Order capture response has purchase_units[].payments.captures[].id
        try
        {
            var client = _httpClientFactory.CreateClient();
            var getOrder = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/v2/checkout/orders/{request.GatewayPaymentId}");
            getOrder.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var orderResponse = await client.SendAsync(getOrder);
            if (!orderResponse.IsSuccessStatusCode)
                return new RefundPaymentResult { Success = false, ErrorCode = orderResponse.StatusCode.ToString(), ErrorMessage = await orderResponse.Content.ReadAsStringAsync() };
            var orderJson = await orderResponse.Content.ReadAsStringAsync();
            var orderDoc = JsonDocument.Parse(orderJson);
            string? captureId = null;
            if (orderDoc.RootElement.TryGetProperty("purchase_units", out var units) && units.GetArrayLength() > 0)
            {
                var first = units[0];
                if (first.TryGetProperty("payments", out var payments) && payments.TryGetProperty("captures", out var captures) && captures.GetArrayLength() > 0)
                    captureId = captures[0].TryGetProperty("id", out var cid) ? cid.GetString() : null;
            }
            if (string.IsNullOrEmpty(captureId))
                return new RefundPaymentResult { Success = false, ErrorCode = "NoCapture", ErrorMessage = "No capture found for this order." };

            object refundPayload;
            if (request.Amount.HasValue && request.Amount.Value > 0)
                refundPayload = new { amount = new { value = request.Amount.Value.ToString("F2"), currency_code = "USD" }, note_to_payer = request.Reason };
            else
                refundPayload = new { note_to_payer = request.Reason };
            var refundReq = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/v2/payments/captures/{captureId}/refund");
            refundReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            refundReq.Content = new StringContent(JsonSerializer.Serialize(refundPayload), Encoding.UTF8, "application/json");
            var refundResponse = await client.SendAsync(refundReq);
            if (!refundResponse.IsSuccessStatusCode)
                return new RefundPaymentResult { Success = false, ErrorCode = refundResponse.StatusCode.ToString(), ErrorMessage = await refundResponse.Content.ReadAsStringAsync() };
            return new RefundPaymentResult { Success = true };
        }
        catch (Exception ex)
        {
            return new RefundPaymentResult { Success = false, ErrorCode = "Exception", ErrorMessage = ex.Message };
        }
    }
}

