using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace EcomMicroService.Basket;

public interface ICartAppService : IApplicationService
{
    Task<CartDto> GetCartAsync(Guid? guestCartId = null);
    Task<CartDto> AddItemAsync(AddCartItemDto input, Guid? guestCartId = null);
    Task<CartDto> UpdateItemAsync(Guid cartItemId, UpdateCartItemDto input, Guid? guestCartId = null);
    Task<CartDto> RemoveItemAsync(Guid cartItemId, Guid? guestCartId = null);
    Task<CartDto> MergeGuestCartAsync(Guid guestCartId);
    Task ClearCartAsync(Guid? guestCartId = null);
}

public class CartDto
{
    public Guid Id { get; set; }
    public bool IsAuthenticated { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public int ItemCount { get; set; }
}

public class CartItemDto
{
    public Guid Id { get; set; }
    public Guid CartId { get; set; }
    public Guid ProductVariantId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal? UnitPrice { get; set; }
    public int Quantity { get; set; }
    public int? AvailableStock { get; set; }
}

public class AddCartItemDto
{
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateCartItemDto
{
    public int Quantity { get; set; }
}
