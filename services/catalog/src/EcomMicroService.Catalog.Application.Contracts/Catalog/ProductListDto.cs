using System;

namespace EcomMicroService.Catalog;

/// <summary>
/// DTO for product list (admin).
/// </summary>
public class ProductListDto
{
    public Guid Id { get; set; }
    public string ProductNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public Guid? BrandId { get; set; }
    public string? BrandName { get; set; }
    public Guid? ModelId { get; set; }
    public string? ModelName { get; set; }
    public Guid? ProductTypeId { get; set; }
    public string? ProductTypeName { get; set; }
    public int RequiredAttributeCount { get; set; }
    public int FilledRequiredAttributeCount { get; set; }
    public bool IsAttributeComplete { get; set; }
    public bool IsPublished { get; set; }
    public decimal? PriceFrom { get; set; }
}
