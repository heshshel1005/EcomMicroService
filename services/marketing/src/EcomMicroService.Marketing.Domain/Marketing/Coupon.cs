using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Marketing;

/// <summary>
/// Coupon: code, type (% or fixed), min order, validity, usage limits.
/// </summary>
public class Coupon : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public CouponType Type { get; set; }
    /// <summary>For Percent: 0–100. For FixedAmount: amount in currency.</summary>
    public decimal Value { get; set; }
    /// <summary>Minimum order subtotal required to apply (0 = no minimum).</summary>
    public decimal MinOrderAmount { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    /// <summary>Total number of times the coupon can be used (null = unlimited).</summary>
    public int? TotalUsageLimit { get; set; }
    /// <summary>Per-user usage limit (null = unlimited).</summary>
    public int? PerUserUsageLimit { get; set; }
    public bool IsActive { get; set; } = true;

    protected Coupon()
    {
    }

    public Coupon(
        Guid id,
        string code,
        CouponType type,
        decimal value,
        decimal minOrderAmount = 0,
        DateTime? validFrom = null,
        DateTime? validTo = null,
        int? totalUsageLimit = null,
        int? perUserUsageLimit = null)
        : base(id)
    {
        Code = code?.Trim().ToUpperInvariant() ?? string.Empty;
        Type = type;
        Value = value;
        MinOrderAmount = minOrderAmount;
        ValidFrom = validFrom;
        ValidTo = validTo;
        TotalUsageLimit = totalUsageLimit;
        PerUserUsageLimit = perUserUsageLimit;
    }

    public bool IsValidAt(DateTime at)
    {
        if (!IsActive) return false;
        if (ValidFrom.HasValue && at < ValidFrom.Value) return false;
        if (ValidTo.HasValue && at > ValidTo.Value) return false;
        return true;
    }

    public decimal CalculateDiscount(decimal subtotal)
    {
        if (subtotal < MinOrderAmount) return 0;
        if (Type == CouponType.Percent)
            return Math.Round(subtotal * (Value / 100m), 2);
        return Math.Min(Value, subtotal);
    }
}

