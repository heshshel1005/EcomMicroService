using System;
using Volo.Abp.Application.Dtos;

namespace EcomMicroService.Catalog;

/// <summary>
/// Request for public (storefront) product list with filters and search.
/// </summary>
public class PublicProductListRequestDto : PagedAndSortedResultRequestDto
{
    /// <summary>Search in product name, description, and product number (case-insensitive).</summary>
    public string? Search { get; set; }

    /// <summary>Filter by category.</summary>
    public Guid? CategoryId { get; set; }

    /// <summary>Minimum price (any variant).</summary>
    public decimal? PriceMin { get; set; }

    /// <summary>Maximum price (any variant).</summary>
    public decimal? PriceMax { get; set; }

    /// <summary>Optional product type context; when set, products are limited to this type.</summary>
    public Guid? ProductTypeId { get; set; }

    /// <summary>
    /// JSON object of metadata-driven dynamic attribute filters (key/value map), e.g. {"condition":"new","fitment_type":"universal"}.
    /// </summary>
    public string? DynamicFiltersJson { get; set; }

    /// <summary>Filter by brand.</summary>
    public Guid? BrandId { get; set; }

    /// <summary>Filter by model (optional; when set, products must match this model).</summary>
    public Guid? ModelId { get; set; }
}
