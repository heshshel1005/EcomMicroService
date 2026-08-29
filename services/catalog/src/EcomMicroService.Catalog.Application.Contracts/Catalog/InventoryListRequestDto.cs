using System;
using Volo.Abp.Application.Dtos;

namespace EcomMicroService.Catalog;

public class InventoryListRequestDto : PagedAndSortedResultRequestDto
{
    /// <summary>Filter by product variant id.</summary>
    public Guid? ProductVariantId { get; set; }
    /// <summary>When true, return only items where quantity is at or below low-stock threshold.</summary>
    public bool LowStockOnly { get; set; }
}
