using System;
using System.Collections.Generic;

namespace EcomMicroService.Catalog;

/// <summary>
/// Product variant (SKU) with attributes and inventory.
/// </summary>
public class ProductVariantDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public int Quantity { get; set; }
    public int Reserved { get; set; }
    public int AvailableQuantity { get; set; }
    public Dictionary<string, object?> DynamicAttributes { get; set; } = new();
    public List<ProductVariantAttributeDto> Attributes { get; set; } = new();
}
