using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Marketing;

/// <summary>
/// Customer wishlist. One per user; items are product variants (WishlistItem).
/// </summary>
public class Wishlist : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid UserId { get; set; }

    protected Wishlist()
    {
    }

    public Wishlist(Guid id, Guid userId)
        : base(id)
    {
        UserId = userId;
    }
}

