using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcomMicroService.Catalog;

/// <summary>
/// Input for creating a product variant (SKU) with attributes and initial inventory.
/// </summary>
public class CreateProductVariantDto
{
    [Required]
    [StringLength(CatalogConsts.Catalog.ProductVariantMaxSkuLength)]
    public string Sku { get; set; } = string.Empty;

    public decimal? Price { get; set; }

    public int Quantity { get; set; }

    public Dictionary<string, object?> DynamicAttributes { get; set; } = new();

    /// <summary>
    /// Attribute values: ProductAttributeId -> Value (e.g. Size -> "M", Color -> "Red").
    /// </summary>
    public List<ProductVariantAttributeInputDto> Attributes { get; set; } = new();
}

/// <summary>
/// Single attribute value for a variant.
/// </summary>
public class ProductVariantAttributeInputDto
{
    public Guid ProductAttributeId { get; set; }
    public string Value { get; set; } = string.Empty;
}
