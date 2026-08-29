using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Basket;

public class Cart : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? AnonymousId { get; set; }

    protected Cart()
    {
    }

    public Cart(Guid id, Guid? userId, Guid? anonymousId)
        : base(id)
    {
        if (userId == null && anonymousId == null)
            throw new ArgumentException("Either UserId or AnonymousId must be set.", nameof(anonymousId));
        if (userId != null && anonymousId != null)
            throw new ArgumentException("Cart cannot have both UserId and AnonymousId.", nameof(anonymousId));

        UserId = userId;
        AnonymousId = anonymousId;
    }
}
