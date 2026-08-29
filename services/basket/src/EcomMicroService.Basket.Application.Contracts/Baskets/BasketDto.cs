using System.Collections.Generic;
using System.Linq;

namespace EcomMicroService.Basket.Baskets;

public class BasketDto
{
    public List<BasketItemDto> Items { get; set; } = new();

    public decimal TotalPrice => Items.Sum(x => x.UnitPrice * x.Quantity);
}
