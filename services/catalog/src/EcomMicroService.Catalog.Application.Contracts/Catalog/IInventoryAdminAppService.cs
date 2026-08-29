using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EcomMicroService.Catalog;

/// <summary>
/// Admin API: inventory CRUD, low-stock list.
/// </summary>
[Volo.Abp.RemoteService(IsEnabled = false)]
public interface IInventoryAdminAppService : IApplicationService
{
    Task<PagedResultDto<InventoryDto>> GetListAsync(InventoryListRequestDto input);
    Task<InventoryDto> GetAsync(Guid id);
    Task<InventoryDto?> GetByVariantIdAsync(Guid productVariantId);
    Task<InventoryDto> UpdateAsync(Guid id, UpdateInventoryDto input);
    /// <summary>Ensure an inventory record exists for the variant; create with optional quantity/threshold if missing.</summary>
    Task<InventoryDto> EnsureForVariantAsync(Guid productVariantId, int quantity = 0, int? lowStockThreshold = null);
}
