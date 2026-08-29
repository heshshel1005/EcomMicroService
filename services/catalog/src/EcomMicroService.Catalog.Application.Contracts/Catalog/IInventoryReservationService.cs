using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace EcomMicroService.Catalog;

/// <summary>
/// Reserves and releases inventory for cart items. Reserve increases Reserved; release decreases it.
/// </summary>
public interface IInventoryReservationService : IApplicationService
{
    /// <summary>
    /// Reserves quantity for a variant (increments Reserved). Throws if AvailableQuantity would go negative.
    /// </summary>
    Task ReserveAsync(Guid productVariantId, int quantity);

    /// <summary>
    /// Releases quantity for a variant (decrements Reserved, never below zero).
    /// </summary>
    Task ReleaseAsync(Guid productVariantId, int quantity);

    /// <summary>
    /// Releases reserved quantities for a set of (variant, quantity) pairs. Groups by variant and sums before releasing.
    /// </summary>
    Task ReleaseForCartItemsAsync(IEnumerable<(Guid ProductVariantId, int Quantity)> items);
}
