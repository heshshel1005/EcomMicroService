using System;

namespace EcomMicroService.Catalog;

/// <summary>
/// Attribute value on a variant (e.g. Size = "M", Color = "Red").
/// </summary>
public class ProductVariantAttributeDto
{
    public Guid ProductAttributeId { get; set; }
    public string ProductAttributeName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
