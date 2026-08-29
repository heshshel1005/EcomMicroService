using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Marketing;

/// <summary>
/// Rule for redeeming loyalty points (e.g. 100 points = 10% off, or free shipping).
/// </summary>
public class RedemptionRule : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public RedemptionRuleType Type { get; set; }
    /// <summary>Points required to redeem this rule.</summary>
    public int PointsRequired { get; set; }
    /// <summary>For DiscountPercent: 0-100. For FixedDiscount: amount in currency. For FreeShipping: ignored.</summary>
    public decimal Value { get; set; }
    /// <summary>Minimum order amount to use this rule (0 = no minimum).</summary>
    public decimal MinOrderAmount { get; set; }
    public bool IsActive { get; set; } = true;

    protected RedemptionRule()
    {
    }

    public RedemptionRule(
        Guid id,
        string name,
        RedemptionRuleType type,
        int pointsRequired,
        decimal value = 0,
        decimal minOrderAmount = 0)
        : base(id)
    {
        Name = name ?? string.Empty;
        Type = type;
        PointsRequired = pointsRequired;
        Value = value;
        MinOrderAmount = minOrderAmount;
    }

    public bool CanApplyTo(decimal orderSubTotal, int customerPoints)
    {
        if (!IsActive) return false;
        if (customerPoints < PointsRequired) return false;
        if (orderSubTotal < MinOrderAmount) return false;
        return true;
    }

    /// <summary>Discount amount when applied (for FixedDiscount or DiscountPercent). For FreeShipping returns 0 (handled separately).</summary>
    public decimal GetDiscountAmount(decimal orderSubTotal)
    {
        if (Type == RedemptionRuleType.FreeShipping) return 0;
        if (Type == RedemptionRuleType.DiscountPercent)
            return Math.Round(orderSubTotal * (Value / 100m), 2);
        return Math.Min(Value, orderSubTotal);
    }
}

