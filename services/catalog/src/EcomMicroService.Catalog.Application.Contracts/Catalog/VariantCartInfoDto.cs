using System;

namespace EcomMicroService.Catalog;

public class VariantCartInfoDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal? UnitPrice { get; set; }
    public int AvailableQuantity { get; set; }
}
