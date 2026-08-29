using System;

namespace EcomMicroService.Catalog;

public class InventoryDto
{
    public Guid Id { get; set; }
    public Guid ProductVariantId { get; set; }
    public string? ProductName { get; set; }
    public string? Sku { get; set; }
    public int Quantity { get; set; }
    public int Reserved { get; set; }
    public int AvailableQuantity { get; set; }
    public int? LowStockThreshold { get; set; }
    public bool IsLowStock { get; set; }
}
