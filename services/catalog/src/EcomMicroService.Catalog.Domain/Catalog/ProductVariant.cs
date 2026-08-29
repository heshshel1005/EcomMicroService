using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Catalog;

/// <summary>
/// Product variant (SKU). Each variant has a unique SKU; attributes (e.g. size, color) are stored in ProductVariantAttribute.
/// Supports multiple sizes or "one size".
/// </summary>
public class ProductVariant : AuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public string? DynamicAttributesJson { get; set; }

    protected ProductVariant()
    {
    }

    public ProductVariant(Guid id, Guid productId, string sku, decimal? price = null, string? dynamicAttributesJson = null)
        : base(id)
    {
        ProductId = productId;
        Sku = sku ?? string.Empty;
        Price = price;
        DynamicAttributesJson = dynamicAttributesJson;
    }
}
