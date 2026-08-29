using System;
using System.Collections.Generic;
using Microsoft.Extensions.Http;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Volo.Abp.DependencyInjection;

namespace EcomMicroService.Ordering;

public class ShopIntegrationClients : ITransientDependency
{
    private readonly HttpClient _basket;
    private readonly HttpClient _catalog;
    private readonly HttpClient _marketing;
    private readonly HttpClient _payment;
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    public ShopIntegrationClients(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _basket = Create(httpClientFactory, configuration, "Basket", "https://localhost:7006/");
        _catalog = Create(httpClientFactory, configuration, "Catalog", "https://localhost:7005/");
        _marketing = Create(httpClientFactory, configuration, "Marketing", "https://localhost:7010/");
        _payment = Create(httpClientFactory, configuration, "Payment", "https://localhost:7009/");
    }

    private static HttpClient Create(IHttpClientFactory factory, IConfiguration configuration, string name, string fallback)
    {
        var http = factory.CreateClient(name);
        var baseUrl = configuration[$"RemoteServices:{name}:BaseUrl"] ?? fallback;
        http.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        return http;
    }

    public async Task<RemoteCartDto> GetCartAsync(Guid? guestCartId)
    {
        var url = guestCartId.HasValue && guestCartId != Guid.Empty
            ? $"api/basket/cart?guestCartId={guestCartId}"
            : "api/basket/cart";
        var cart = await _basket.GetFromJsonAsync<RemoteCartDto>(url, Json);
        return cart ?? new RemoteCartDto();
    }

    public async Task ClearCartAsync(Guid? guestCartId)
    {
        var url = guestCartId.HasValue && guestCartId != Guid.Empty
            ? $"api/basket/cart/clear?guestCartId={guestCartId}"
            : "api/basket/cart/clear";
        var response = await _basket.PostAsync(url, content: null);
        response.EnsureSuccessStatusCode();
    }

    public async Task ValidateVariantAsync(Guid productVariantId, int quantity)
    {
        var response = await _catalog.PostAsync(
            $"api/catalog/inventory-validation/validate-variant-availability?productVariantId={productVariantId}&quantity={quantity}",
            content: null);
        response.EnsureSuccessStatusCode();
    }

    public async Task ReleaseReservationsAsync(IEnumerable<InventoryLineDto> lines)
    {
        var response = await _catalog.PostAsJsonAsync("api/catalog/inventory-deduction/release-reservations", lines);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeductInventoryAsync(IEnumerable<InventoryLineDto> lines)
    {
        var response = await _catalog.PostAsJsonAsync("api/catalog/inventory-deduction/deduct", lines);
        response.EnsureSuccessStatusCode();
    }

    public async Task RestoreInventoryAsync(IEnumerable<InventoryLineDto> lines)
    {
        var response = await _catalog.PostAsJsonAsync("api/catalog/inventory-deduction/restore", lines);
        response.EnsureSuccessStatusCode();
    }

    public async Task<CouponPreviewDto?> PreviewCouponAsync(string code, decimal subtotal)
    {
        var response = await _marketing.GetAsync(
            $"api/marketing/coupons/preview?code={Uri.EscapeDataString(code)}&subtotal={subtotal}");
        if (!response.IsSuccessStatusCode)
            return null;
        return await response.Content.ReadFromJsonAsync<CouponPreviewDto>(Json);
    }

    public async Task RecordCouponUsageAsync(Guid couponId, Guid? userId, Guid orderId)
    {
        var response = await _marketing.PostAsJsonAsync("api/marketing/coupons/record-usage", new
        {
            couponId,
            userId,
            orderId
        });
        response.EnsureSuccessStatusCode();
    }

    public async Task AwardLoyaltyAsync(Guid orderId, Guid? userId, decimal total)
    {
        try
        {
            await _marketing.PostAsJsonAsync("api/marketing/loyalty/award-order", new { orderId, userId, orderTotal = total });
        }
        catch
        {
            // optional
        }
    }

    public async Task<RefundRemoteResult> RefundViaPaymentAsync(Guid orderId, decimal? amount, string? reason)
    {
        var response = await _payment.PostAsJsonAsync("api/payment/refund", new { orderId, amount, reason });
        if (!response.IsSuccessStatusCode)
            return new RefundRemoteResult { Success = false, ErrorMessage = response.ReasonPhrase };
        return await response.Content.ReadFromJsonAsync<RefundRemoteResult>(Json) ?? new RefundRemoteResult { Success = false };
    }

    public class RemoteCartDto
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public List<RemoteCartItemDto> Items { get; set; } = new();
        public int ItemCount { get; set; }
    }

    public class RemoteCartItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductVariantId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public decimal? UnitPrice { get; set; }
        public int Quantity { get; set; }
    }

    public class InventoryLineDto
    {
        public Guid ProductVariantId { get; set; }
        public int Quantity { get; set; }
    }

    public class CouponPreviewDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public bool IsValid { get; set; }
    }

    public class RefundRemoteResult
    {
        public bool Success { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
