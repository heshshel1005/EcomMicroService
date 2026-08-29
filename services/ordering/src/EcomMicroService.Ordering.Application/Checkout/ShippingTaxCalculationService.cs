using System.Collections.Generic;
using System.Threading.Tasks;
using EcomMicroService.Ordering.Checkout;
using Volo.Abp.Application.Services;

namespace EcomMicroService.Ordering;

public class ShippingTaxCalculationService : ApplicationService, IShippingTaxCalculationService
{
    private const decimal StandardAmount = 5.00m;
    private const decimal ExpressAmount = 12.00m;
    private const decimal TaxRate = 0.10m;

    public Task<List<ShippingOptionDto>> GetShippingOptionsAsync(decimal subtotal, string? countryCode = null, string? regionCode = null)
    {
        return Task.FromResult(new List<ShippingOptionDto>
        {
            new() { Code = "standard", Name = "Standard shipping", Amount = StandardAmount },
            new() { Code = "express", Name = "Express shipping", Amount = ExpressAmount },
        });
    }

    public Task<decimal> CalculateTaxAsync(decimal subtotal, string? countryCode = null, string? regionCode = null)
    {
        return Task.FromResult(subtotal * TaxRate);
    }
}
