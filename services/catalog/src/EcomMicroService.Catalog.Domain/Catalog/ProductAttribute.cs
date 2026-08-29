using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Catalog;

/// <summary>
/// Attribute type for products (e.g. Size, Color). Used to define variant dimensions;
/// supports multiple sizes (S, M, L) or "one size".
/// </summary>
public class ProductAttribute : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public string Name { get; set; } = string.Empty;

    protected ProductAttribute()
    {
    }

    public ProductAttribute(Guid id, string name)
        : base(id)
    {
        Name = name ?? string.Empty;
    }
}
