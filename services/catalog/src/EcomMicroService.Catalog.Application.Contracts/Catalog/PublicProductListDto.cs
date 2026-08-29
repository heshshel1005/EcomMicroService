using System;

namespace EcomMicroService.Catalog;

/// <summary>
/// DTO for a product in the public (storefront) list.
/// </summary>
public class PublicProductListDto
{
    public Guid Id { get; set; }
    public string ProductNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public decimal? PriceFrom { get; set; }
    /// <summary>True if at least one variant has available stock.</summary>
    public bool IsInStock { get; set; }
    /// <summary>Primary image media ID for storefront card; null if no image.</summary>
    public Guid? PrimaryMediaId { get; set; }
    /// <summary>Brand ID (product structure).</summary>
    public Guid BrandId { get; set; }
    /// <summary>Brand name for display.</summary>
    public string? BrandName { get; set; }
    /// <summary>Optional model ID.</summary>
    public Guid? ModelId { get; set; }
    /// <summary>Model name for display when ModelId is set.</summary>
    public string? ModelName { get; set; }
}
