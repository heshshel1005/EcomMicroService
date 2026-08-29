using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace EcomMicroService.Ordering.Checkout;

[Volo.Abp.RemoteService(IsEnabled = false)]
public interface ICheckoutAppService : IApplicationService
{
    Task<CheckoutSummaryDto> GetSummaryAsync(Guid? guestCartId = null, string? couponCode = null);
    Task<SubmitCheckoutResultDto> SubmitOrderAsync(SubmitCheckoutDto input, Guid? guestCartId = null);
}

public class CheckoutAddressDto
{
    [Required]
    [StringLength(512)]
    public string Street { get; set; } = string.Empty;
    [StringLength(512)]
    public string? Street2 { get; set; }
    [StringLength(128)]
    public string? City { get; set; }
    [StringLength(128)]
    public string? Region { get; set; }
    [StringLength(32)]
    public string? PostalCode { get; set; }
    [StringLength(128)]
    public string? Country { get; set; }
    [StringLength(500)]
    public string? DeliveryInstructions { get; set; }
}

public class SubmitCheckoutDto
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string ContactEmail { get; set; } = string.Empty;
    [StringLength(32)]
    public string? ContactPhone { get; set; }
    [StringLength(256)]
    public string? ContactName { get; set; }
    [Required]
    public CheckoutAddressDto ShippingAddress { get; set; } = null!;
    public bool BillingSameAsShipping { get; set; } = true;
    public CheckoutAddressDto? BillingAddress { get; set; }
    [Required]
    [StringLength(64)]
    public string ShippingMethodCode { get; set; } = string.Empty;
    [StringLength(64)]
    public string? CouponCode { get; set; }
}

public class SubmitCheckoutResultDto
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = "Pending";
}

public class ShippingOptionDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class CheckoutCartItemDto
{
    public Guid Id { get; set; }
    public Guid ProductVariantId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal? UnitPrice { get; set; }
    public int Quantity { get; set; }
}

public class CheckoutCartDto
{
    public Guid Id { get; set; }
    public List<CheckoutCartItemDto> Items { get; set; } = new();
    public int ItemCount { get; set; }
}

public class CheckoutSummaryDto
{
    public CheckoutCartDto Cart { get; set; } = null!;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? AppliedCouponCode { get; set; }
    public List<ShippingOptionDto> ShippingOptions { get; set; } = new();
    public decimal TaxAmount { get; set; }
    public string? DefaultShippingMethodCode { get; set; }
}

public interface IShippingTaxCalculationService : IApplicationService
{
    Task<List<ShippingOptionDto>> GetShippingOptionsAsync(decimal subtotal, string? countryCode = null, string? regionCode = null);
    Task<decimal> CalculateTaxAsync(decimal subtotal, string? countryCode = null, string? regionCode = null);
}
