using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Marketing;

/// <summary>
/// Record of a loyalty points earn or spend (e.g. points per order, redemption).
/// </summary>
public class PointsTransaction : CreationAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid UserId { get; set; }
    /// <summary>Positive for earn, negative for spend.</summary>
    public int Amount { get; set; }
    public PointsTransactionType Type { get; set; }
    /// <summary>Order id when points were earned or spent for that order.</summary>
    public Guid? OrderId { get; set; }
    /// <summary>Redemption rule used when Type is Spend.</summary>
    public Guid? RedemptionRuleId { get; set; }
    public string? Description { get; set; }

    protected PointsTransaction()
    {
    }

    public PointsTransaction(
        Guid id,
        Guid userId,
        int amount,
        PointsTransactionType type,
        Guid? orderId = null,
        Guid? redemptionRuleId = null,
        string? description = null)
        : base(id)
    {
        UserId = userId;
        Amount = amount;
        Type = type;
        OrderId = orderId;
        RedemptionRuleId = redemptionRuleId;
        Description = description;
    }
}

