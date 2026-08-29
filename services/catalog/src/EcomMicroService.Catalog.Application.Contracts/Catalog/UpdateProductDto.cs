using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcomMicroService.Catalog;

/// <summary>
/// Input for updating a product. Variants are replaced: list defines current set (add/update/delete).
/// </summary>
public class UpdateProductDto
{
    [Required]
    [StringLength(CatalogConsts.Catalog.ProductMaxProductNumberLength)]
    public string ProductNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(CatalogConsts.Catalog.ProductMaxNameLength)]
    public string Name { get; set; } = string.Empty;

    [StringLength(CatalogConsts.Catalog.ProductMaxDescriptionLength)]
    public string? Description { get; set; }

    public Guid? CategoryId { get; set; }

    [Required]
    public Guid BrandId { get; set; }

    public Guid? ModelId { get; set; }

    public Guid? ProductTypeId { get; set; }

    public Dictionary<string, object?> DynamicAttributes { get; set; } = new();

    public bool IsPublished { get; set; }

    public List<ProductTranslationDto> Translations { get; set; } = new();

    public List<UpdateProductVariantDto> Variants { get; set; } = new();
}
