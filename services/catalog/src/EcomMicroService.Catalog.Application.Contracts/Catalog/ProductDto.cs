using System;
using System.Collections.Generic;

namespace EcomMicroService.Catalog;

/// <summary>
/// Full product DTO for get/edit (admin).
/// </summary>
public class ProductDto
{
    public Guid Id { get; set; }
    public string ProductNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid BrandId { get; set; }
    public Guid? ModelId { get; set; }
    /// <summary>Optional product type for metadata-driven dynamic attributes.</summary>
    public Guid? ProductTypeId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string? ModelName { get; set; }
    /// <summary>Product-level dynamic attributes JSON (legacy); variants may hold variant-specific values.</summary>
    public Dictionary<string, object?> DynamicAttributes { get; set; } = new();
    public bool IsPublished { get; set; }
    /// <summary>Primary image media id for PDP; null if none.</summary>
    public Guid? PrimaryMediaId { get; set; }
    /// <summary>All image media ids for PDP gallery (backward compatibility).</summary>
    public List<Guid> MediaIds { get; set; } = new();
    /// <summary>All media (images and videos) for PDP gallery with type.</summary>
    public List<ProductMediaItemDto> Media { get; set; } = new();
    public List<ProductVariantDto> Variants { get; set; } = new();
    public List<ProductTranslationDto> Translations { get; set; } = new();
}
