using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EcomMicroService.Catalog.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace EcomMicroService.Catalog;

/// <summary>
/// Admin: inventory list (with low-stock filter), get, update, ensure-for-variant.
/// </summary>
[Volo.Abp.RemoteService(IsEnabled = false)]
[Authorize(CatalogPermissions.Administration)]
public class InventoryAdminAppService : CatalogAppService, IInventoryAdminAppService
{
    private readonly IRepository<Inventory, Guid> _inventoryRepository;
    private readonly IRepository<ProductVariant, Guid> _variantRepository;
    private readonly IRepository<Product, Guid> _productRepository;

    public InventoryAdminAppService(
        IRepository<Inventory, Guid> inventoryRepository,
        IRepository<ProductVariant, Guid> variantRepository,
        IRepository<Product, Guid> productRepository)
    {
        _inventoryRepository = inventoryRepository;
        _variantRepository = variantRepository;
        _productRepository = productRepository;
    }

    public async Task<PagedResultDto<InventoryDto>> GetListAsync(InventoryListRequestDto input)
    {
        var query = await _inventoryRepository.GetQueryableAsync();
        if (input.ProductVariantId.HasValue)
            query = query.Where(i => i.ProductVariantId == input.ProductVariantId.Value);
        if (input.LowStockOnly)
            query = query.Where(i => i.LowStockThreshold != null && i.Quantity <= i.LowStockThreshold.Value);

        var total = await AsyncExecuter.CountAsync(query);
        var sortBy = (input.Sorting ?? "CreationTime").Replace(" DESC", "", StringComparison.OrdinalIgnoreCase).Trim();
        query = sortBy switch
        {
            "Quantity" => input.Sorting?.Contains("DESC", StringComparison.OrdinalIgnoreCase) == true ? query.OrderByDescending(i => i.Quantity) : query.OrderBy(i => i.Quantity),
            "ProductVariantId" => input.Sorting?.Contains("DESC", StringComparison.OrdinalIgnoreCase) == true ? query.OrderByDescending(i => i.ProductVariantId) : query.OrderBy(i => i.ProductVariantId),
            _ => input.Sorting?.Contains("DESC", StringComparison.OrdinalIgnoreCase) == true ? query.OrderByDescending(i => i.CreationTime) : query.OrderBy(i => i.CreationTime),
        };
        var skip = input.SkipCount;
        var take = input.MaxResultCount > 0 ? input.MaxResultCount : 10;
        var list = await AsyncExecuter.ToListAsync(query.Skip(skip).Take(take));
        var variantIds = list.Select(i => i.ProductVariantId).Distinct().ToList();
        var variants = await _variantRepository.GetListAsync(v => variantIds.Contains(v.Id));
        var productIds = variants.Select(v => v.ProductId).Distinct().ToList();
        var products = await _productRepository.GetListAsync(p => productIds.Contains(p.Id));
        var variantMap = variants.ToDictionary(v => v.Id);
        var productMap = products.ToDictionary(p => p.Id);

        var items = list.Select(inv =>
        {
            variantMap.TryGetValue(inv.ProductVariantId, out var variant);
            var product = variant != null && productMap.TryGetValue(variant.ProductId, out var p) ? p : null;
            return new InventoryDto
            {
                Id = inv.Id,
                ProductVariantId = inv.ProductVariantId,
                ProductName = product?.Name,
                Sku = variant?.Sku,
                Quantity = inv.Quantity,
                Reserved = inv.Reserved,
                AvailableQuantity = inv.AvailableQuantity,
                LowStockThreshold = inv.LowStockThreshold,
                IsLowStock = inv.IsLowStock,
            };
        }).ToList();

        return new PagedResultDto<InventoryDto>(total, items);
    }

    public async Task<InventoryDto> GetAsync(Guid id)
    {
        var inv = await _inventoryRepository.GetAsync(id);
        return await MapToDtoAsync(inv);
    }

    public async Task<InventoryDto?> GetByVariantIdAsync(Guid productVariantId)
    {
        var inv = await _inventoryRepository.FirstOrDefaultAsync(i => i.ProductVariantId == productVariantId);
        if (inv == null) return null;
        return await MapToDtoAsync(inv);
    }

    public async Task<InventoryDto> UpdateAsync(Guid id, UpdateInventoryDto input)
    {
        var inv = await _inventoryRepository.GetAsync(id);
        if (input.Quantity.HasValue)
            inv.Quantity = input.Quantity.Value;
        if (input.Reserved.HasValue)
            inv.Reserved = Math.Max(0, input.Reserved.Value);
        if (input.LowStockThreshold.HasValue)
            inv.LowStockThreshold = input.LowStockThreshold.Value >= 0 ? input.LowStockThreshold : null;
        await _inventoryRepository.UpdateAsync(inv);
        return await MapToDtoAsync(inv);
    }

    public async Task<InventoryDto> EnsureForVariantAsync(Guid productVariantId, int quantity = 0, int? lowStockThreshold = null)
    {
        var inv = await _inventoryRepository.FirstOrDefaultAsync(i => i.ProductVariantId == productVariantId);
        if (inv != null)
            return await MapToDtoAsync(inv);
        inv = new Inventory(GuidGenerator.Create(), productVariantId, quantity, 0, lowStockThreshold);
        await _inventoryRepository.InsertAsync(inv);
        return await MapToDtoAsync(inv);
    }

    private async Task<InventoryDto> MapToDtoAsync(Inventory inv)
    {
        var variant = await _variantRepository.FirstOrDefaultAsync(v => v.Id == inv.ProductVariantId);
        var product = variant != null ? await _productRepository.FirstOrDefaultAsync(p => p.Id == variant.ProductId) : null;
        return new InventoryDto
        {
            Id = inv.Id,
            ProductVariantId = inv.ProductVariantId,
            ProductName = product?.Name,
            Sku = variant?.Sku,
            Quantity = inv.Quantity,
            Reserved = inv.Reserved,
            AvailableQuantity = inv.AvailableQuantity,
            LowStockThreshold = inv.LowStockThreshold,
            IsLowStock = inv.IsLowStock,
        };
    }
}
