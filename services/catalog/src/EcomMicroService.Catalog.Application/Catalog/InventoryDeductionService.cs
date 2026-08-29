using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace EcomMicroService.Catalog;

/// <summary>
/// Deducts inventory when an order is confirmed.
/// </summary>
public class InventoryDeductionService : CatalogAppService, IInventoryDeductionService
{
    private readonly IRepository<Inventory, Guid> _inventoryRepository;

    public InventoryDeductionService(IRepository<Inventory, Guid> inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task DeductForOrderLinesAsync(IEnumerable<(Guid ProductVariantId, int Quantity)> lines)
    {
        var grouped = lines
            .Where(x => x.Quantity > 0)
            .GroupBy(x => x.ProductVariantId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        if (grouped.Count == 0)
            return;

        var variantIds = grouped.Keys.ToList();
        var inventories = await _inventoryRepository.GetListAsync(i => variantIds.Contains(i.ProductVariantId));

        foreach (var (variantId, qty) in grouped)
        {
            var inv = inventories.FirstOrDefault(i => i.ProductVariantId == variantId);
            var available = inv?.AvailableQuantity ?? 0;
            if (available < qty)
                throw new Volo.Abp.BusinessException("ECommerce:InsufficientStock")
                    .WithData("ProductVariantId", variantId)
                    .WithData("Requested", qty)
                    .WithData("Available", available);
        }

        foreach (var inv in inventories)
        {
            var qty = grouped.GetValueOrDefault(inv.ProductVariantId, 0);
            if (qty <= 0) continue;
            inv.Quantity -= qty;
            if (inv.Quantity < 0)
                inv.Quantity = 0;
            await _inventoryRepository.UpdateAsync(inv);
        }
    }

    public async Task RestoreForOrderLinesAsync(IEnumerable<(Guid ProductVariantId, int Quantity)> lines)
    {
        var grouped = lines
            .Where(x => x.Quantity > 0)
            .GroupBy(x => x.ProductVariantId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        if (grouped.Count == 0)
            return;

        var variantIds = grouped.Keys.ToList();
        var inventories = await _inventoryRepository.GetListAsync(i => variantIds.Contains(i.ProductVariantId));

        foreach (var inv in inventories)
        {
            var qty = grouped.GetValueOrDefault(inv.ProductVariantId, 0);
            if (qty <= 0) continue;
            inv.Quantity += qty;
            await _inventoryRepository.UpdateAsync(inv);
        }
    }
}
