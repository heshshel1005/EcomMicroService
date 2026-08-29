using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Marketing;

/// <summary>
/// A product variant on a customer's wishlist.
/// </summary>
public class WishlistItem : AuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid WishlistId { get; set; }
    public Guid ProductVariantId { get; set; }

    protected WishlistItem()
    {
    }

    public WishlistItem(Guid id, Guid wishlistId, Guid productVariantId)
        : base(id)
    {
        WishlistId = wishlistId;
        ProductVariantId = productVariantId;
    }
}

