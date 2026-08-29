using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace EcomMicroService.Basket;

[RemoteService(Name = "Basket")]
[Area("basket")]
[Route("api/basket/cart")]
public class PersistentCartController : BasketController, ICartAppService
{
    private readonly ICartAppService _cartAppService;

    public PersistentCartController(ICartAppService cartAppService)
    {
        _cartAppService = cartAppService;
    }

    [HttpGet]
    public Task<CartDto> GetCartAsync(Guid? guestCartId = null) => _cartAppService.GetCartAsync(guestCartId);

    [HttpPost("items")]
    public Task<CartDto> AddItemAsync(AddCartItemDto input, Guid? guestCartId = null) =>
        _cartAppService.AddItemAsync(input, guestCartId);

    [HttpPut("items/{cartItemId}")]
    public Task<CartDto> UpdateItemAsync(Guid cartItemId, UpdateCartItemDto input, Guid? guestCartId = null) =>
        _cartAppService.UpdateItemAsync(cartItemId, input, guestCartId);

    [HttpDelete("items/{cartItemId}")]
    public Task<CartDto> RemoveItemAsync(Guid cartItemId, Guid? guestCartId = null) =>
        _cartAppService.RemoveItemAsync(cartItemId, guestCartId);

    [HttpPost("merge")]
    public Task<CartDto> MergeGuestCartAsync(Guid guestCartId) => _cartAppService.MergeGuestCartAsync(guestCartId);

    [HttpPost("clear")]
    public Task ClearCartAsync(Guid? guestCartId = null) => _cartAppService.ClearCartAsync(guestCartId);
}
