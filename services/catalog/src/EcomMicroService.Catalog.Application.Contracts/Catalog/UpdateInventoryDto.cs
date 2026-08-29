namespace EcomMicroService.Catalog;

public class UpdateInventoryDto
{
    public int? Quantity { get; set; }
    public int? Reserved { get; set; }
    /// <summary>When set, quantity at or below this value is considered low stock.</summary>
    public int? LowStockThreshold { get; set; }
}
