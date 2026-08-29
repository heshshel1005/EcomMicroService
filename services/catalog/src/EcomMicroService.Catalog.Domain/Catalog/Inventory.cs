using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Catalog;

/// <summary>
/// Inventory per product variant: quantity on hand and reserved (e.g. for cart).
/// </summary>
public class Inventory : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; }
    public int Reserved { get; set; }
    /// <summary>When set, quantity at or below this value is considered low stock for alerts.</summary>
    public int? LowStockThreshold { get; set; }

    protected Inventory()
    {
    }

    public Inventory(Guid id, Guid productVariantId, int quantity = 0, int reserved = 0, int? lowStockThreshold = null)
        : base(id)
    {
        ProductVariantId = productVariantId;
        Quantity = quantity;
        Reserved = reserved;
        LowStockThreshold = lowStockThreshold;
    }

    public int AvailableQuantity => Quantity - Reserved;

    /// <summary>True when LowStockThreshold is set and Quantity is at or below it.</summary>
    public bool IsLowStock => LowStockThreshold.HasValue && Quantity <= LowStockThreshold.Value;
}
