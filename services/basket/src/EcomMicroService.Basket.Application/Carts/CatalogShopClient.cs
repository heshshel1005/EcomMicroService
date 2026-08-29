using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Volo.Abp.DependencyInjection;

namespace EcomMicroService.Basket;

public class CatalogShopClient : ITransientDependency
{
    private readonly HttpClient _http;

    public CatalogShopClient(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _http = httpClientFactory.CreateClient("Catalog");
        var baseUrl = configuration["RemoteServices:Catalog:BaseUrl"] ?? "https://localhost:7005/";
        _http.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    }

    public async Task ValidateVariantAvailabilityAsync(Guid productVariantId, int quantity)
    {
        var response = await _http.PostAsync(
            $"api/catalog/inventory-validation/validate-variant-availability?productVariantId={productVariantId}&quantity={quantity}",
            content: null);
        response.EnsureSuccessStatusCode();
    }

    public async Task ReserveAsync(Guid productVariantId, int quantity)
    {
        var response = await _http.PostAsync(
            $"api/catalog/inventory-reservation/reserve?productVariantId={productVariantId}&quantity={quantity}",
            content: null);
        response.EnsureSuccessStatusCode();
    }

    public async Task ReleaseAsync(Guid productVariantId, int quantity)
    {
        var response = await _http.PostAsync(
            $"api/catalog/inventory-reservation/release?productVariantId={productVariantId}&quantity={quantity}",
            content: null);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<VariantCartInfoDto>> GetVariantCartInfoAsync(IEnumerable<Guid> variantIds)
    {
        var ids = string.Join(",", variantIds);
        if (string.IsNullOrWhiteSpace(ids))
        {
            return new List<VariantCartInfoDto>();
        }

        var items = await _http.GetFromJsonAsync<List<VariantCartInfoDto>>(
            $"api/catalog/public-catalog/variants/cart-info?ids={ids}");
        return items ?? new List<VariantCartInfoDto>();
    }

    public class VariantCartInfoDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public decimal? UnitPrice { get; set; }
        public int AvailableQuantity { get; set; }
    }
}
