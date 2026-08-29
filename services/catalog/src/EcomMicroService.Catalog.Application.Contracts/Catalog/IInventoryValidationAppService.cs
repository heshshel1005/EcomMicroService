using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace EcomMicroService.Catalog;

/// <summary>
/// Validates product variant stock availability. Use at add-to-cart and order submit (Plan 3).
/// </summary>
public interface IInventoryValidationAppService : IApplicationService
{
    /// <summary>
    /// Validates that the variant has at least the requested quantity available.
    /// Throws <see cref="Volo.Abp.BusinessException"/> with code ECommerce:InsufficientStock when not enough stock.
    /// </summary>
    Task ValidateVariantAvailabilityAsync(Guid productVariantId, int quantity);
}
