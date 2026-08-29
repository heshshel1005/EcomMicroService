using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Catalog;

/// <summary>
/// Links a product variant to an attribute value (e.g. Size = "M", Color = "Red", or Size = "One Size").
/// </summary>
public class ProductVariantAttribute : AuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid ProductVariantId { get; set; }
    public Guid ProductAttributeId { get; set; }
    public string Value { get; set; } = string.Empty;

    protected ProductVariantAttribute()
    {
    }

    public ProductVariantAttribute(Guid id, Guid productVariantId, Guid productAttributeId, string value)
        : base(id)
    {
        ProductVariantId = productVariantId;
        ProductAttributeId = productAttributeId;
        Value = value ?? string.Empty;
    }
}
