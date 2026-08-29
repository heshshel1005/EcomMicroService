using System;
using System.Threading.Tasks;
using EcomMicroService.Catalog;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace EcomMicroService.Catalog;

/// <summary>
/// Admin inventory API: list (with low-stock filter), get, update, ensure-for-variant.
/// </summary>
[Route("api/catalog/inventory-admin")]
[Area("catalog")]
public class InventoryAdminController : CatalogController
{
    private readonly IInventoryAdminAppService _appService;

    public InventoryAdminController(IInventoryAdminAppService appService)
    {
        _appService = appService;
    }

    [HttpGet]
    public async Task<PagedResultDto<InventoryDto>> GetListAsync([FromQuery] InventoryListRequestDto input)
    {
        return await _appService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    public async Task<InventoryDto> GetAsync(Guid id)
    {
        return await _appService.GetAsync(id);
    }

    [HttpGet("by-variant/{productVariantId}")]
    public async Task<InventoryDto?> GetByVariantIdAsync(Guid productVariantId)
    {
        return await _appService.GetByVariantIdAsync(productVariantId);
    }

    [HttpPut("{id}")]
    public async Task<InventoryDto> UpdateAsync(Guid id, [FromBody] UpdateInventoryDto input)
    {
        return await _appService.UpdateAsync(id, input);
    }

    [HttpPost("ensure-for-variant")]
    public async Task<InventoryDto> EnsureForVariantAsync([FromQuery] Guid productVariantId, [FromQuery] int quantity = 0, [FromQuery] int? lowStockThreshold = null)
    {
        return await _appService.EnsureForVariantAsync(productVariantId, quantity, lowStockThreshold);
    }
}
