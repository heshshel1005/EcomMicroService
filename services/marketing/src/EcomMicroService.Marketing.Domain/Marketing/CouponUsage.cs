using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Marketing;

/// <summary>
/// Tracks coupon usage per user for per-user limits.
/// </summary>
public class CouponUsage : AuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid CouponId { get; set; }
    public Guid? UserId { get; set; }
    public Guid OrderId { get; set; }

    protected CouponUsage()
    {
    }

    public CouponUsage(Guid id, Guid couponId, Guid? userId, Guid orderId)
        : base(id)
    {
        CouponId = couponId;
        UserId = userId;
        OrderId = orderId;
    }
}

