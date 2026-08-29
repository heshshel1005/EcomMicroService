using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcomMicroService.Catalog;

/// <summary>
/// Input for updating a product variant. Id is optional; if missing, treated as new variant.
/// </summary>
public class UpdateProductVariantDto
{
    /// <summary>
    /// Existing variant id; if null, a new variant is created.
    /// </summary>
    public Guid? Id { get; set; }

    [Required]
    [StringLength(CatalogConsts.Catalog.ProductVariantMaxSkuLength)]
    public string Sku { get; set; } = string.Empty;

    public decimal? Price { get; set; }

    public int Quantity { get; set; }

    public Dictionary<string, object?> DynamicAttributes { get; set; } = new();

    public List<ProductVariantAttributeInputDto> Attributes { get; set; } = new();
}
