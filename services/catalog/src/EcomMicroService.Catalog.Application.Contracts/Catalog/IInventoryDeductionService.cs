using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace EcomMicroService.Catalog;

/// <summary>
/// Deducts or restores inventory for orders. Used by Orders admin.
/// </summary>
public interface IInventoryDeductionService : IApplicationService
{
    /// <summary>
    /// Deducts quantity from inventory for each variant in the given order lines.
    /// Throws if any variant has insufficient stock.
    /// </summary>
    Task DeductForOrderLinesAsync(IEnumerable<(Guid ProductVariantId, int Quantity)> lines);

    /// <summary>
    /// Restores quantity to inventory for each variant (e.g. when an order is cancelled after confirmation).
    /// </summary>
    Task RestoreForOrderLinesAsync(IEnumerable<(Guid ProductVariantId, int Quantity)> lines);
}
