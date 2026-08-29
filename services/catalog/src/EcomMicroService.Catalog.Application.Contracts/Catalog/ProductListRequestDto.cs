using System;
using Volo.Abp.Application.Dtos;

namespace EcomMicroService.Catalog;

/// <summary>
/// Request for paged product list with optional filters.
/// </summary>
public class ProductListRequestDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? CategoryId { get; set; }
    public bool? IsPublished { get; set; }
}
