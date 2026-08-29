namespace EcomMicroService.Marketing;

/// <summary>
/// How loyalty points can be redeemed.
/// </summary>
public enum RedemptionRuleType
{
    /// <summary>X points = Y% off order.</summary>
    DiscountPercent = 0,
    /// <summary>X points = fixed amount off.</summary>
    FixedDiscount = 1,
    /// <summary>X points = free shipping.</summary>
    FreeShipping = 2,
}

