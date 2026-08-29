using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace EcomMicroService.Catalog;

[AllowAnonymous]
public class InventoryReservationService : CatalogAppService, IInventoryReservationService
{
    private readonly IRepository<Inventory, Guid> _inventoryRepository;

    public InventoryReservationService(IRepository<Inventory, Guid> inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task ReserveAsync(Guid productVariantId, int quantity)
    {
        if (quantity <= 0) return;

        var inv = await _inventoryRepository.FirstOrDefaultAsync(i => i.ProductVariantId == productVariantId);
        var available = inv?.AvailableQuantity ?? 0;
        if (available < quantity)
            throw new Volo.Abp.BusinessException("ECommerce:InsufficientStock")
                .WithData("ProductVariantId", productVariantId)
                .WithData("Requested", quantity)
                .WithData("Available", available);

        inv!.Reserved += quantity;
        await _inventoryRepository.UpdateAsync(inv);
    }

    public async Task ReleaseAsync(Guid productVariantId, int quantity)
    {
        if (quantity <= 0) return;

        var inv = await _inventoryRepository.FirstOrDefaultAsync(i => i.ProductVariantId == productVariantId);
        if (inv == null) return;

        inv.Reserved = Math.Max(0, inv.Reserved - quantity);
        await _inventoryRepository.UpdateAsync(inv);
    }

    public async Task ReleaseForCartItemsAsync(IEnumerable<(Guid ProductVariantId, int Quantity)> items)
    {
        var grouped = items
            .Where(x => x.Quantity > 0)
            .GroupBy(x => x.ProductVariantId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        if (grouped.Count == 0) return;

        foreach (var (variantId, qty) in grouped)
            await ReleaseAsync(variantId, qty);
    }
}
