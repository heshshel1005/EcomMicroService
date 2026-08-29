using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Marketing;

/// <summary>
/// An item on a gift registry: product variant, desired quantity, optional note.
/// </summary>
public class GiftRegistryItem : AuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid GiftRegistryId { get; set; }
    public Guid ProductVariantId { get; set; }
    public int DesiredQuantity { get; set; }
    public string? Note { get; set; }
    /// <summary>Quantity already purchased/claimed by others.</summary>
    public int QuantityClaimed { get; set; }

    protected GiftRegistryItem()
    {
    }

    public GiftRegistryItem(Guid id, Guid giftRegistryId, Guid productVariantId, int desiredQuantity, string? note = null)
        : base(id)
    {
        GiftRegistryId = giftRegistryId;
        ProductVariantId = productVariantId;
        DesiredQuantity = Math.Max(1, desiredQuantity);
        Note = note;
    }

    public int QuantityRemaining => Math.Max(0, DesiredQuantity - QuantityClaimed);
}

