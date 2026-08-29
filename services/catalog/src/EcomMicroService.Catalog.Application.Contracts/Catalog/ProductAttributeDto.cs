using System;

namespace EcomMicroService.Catalog;

/// <summary>
/// Product attribute (e.g. Size, Color) for admin dropdowns.
/// </summary>
public class ProductAttributeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
