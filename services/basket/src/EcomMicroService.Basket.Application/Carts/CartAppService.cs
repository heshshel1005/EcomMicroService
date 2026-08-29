using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace EcomMicroService.Basket;

public class CartAppService : ApplicationService, ICartAppService
{
    private readonly IRepository<Cart, Guid> _cartRepository;
    private readonly IRepository<CartItem, Guid> _cartItemRepository;
    private readonly CatalogShopClient _catalog;

    public CartAppService(
        IRepository<Cart, Guid> cartRepository,
        IRepository<CartItem, Guid> cartItemRepository,
        CatalogShopClient catalog)
    {
        _cartRepository = cartRepository;
        _cartItemRepository = cartItemRepository;
        _catalog = catalog;
    }

    [AllowAnonymous]
    public async Task<CartDto> GetCartAsync(Guid? guestCartId = null)
    {
        var cart = await ResolveOrCreateCartAsync(guestCartId);
        return await BuildCartDtoAsync(cart);
    }

    [AllowAnonymous]
    public async Task<CartDto> AddItemAsync(AddCartItemDto input, Guid? guestCartId = null)
    {
        await _catalog.ValidateVariantAvailabilityAsync(input.ProductVariantId, input.Quantity);

        var cart = await ResolveOrCreateCartAsync(guestCartId);
        var existing = await _cartItemRepository.FirstOrDefaultAsync(x =>
            x.CartId == cart.Id && x.ProductVariantId == input.ProductVariantId);

        if (existing != null)
        {
            var newQty = existing.Quantity + input.Quantity;
            await _catalog.ValidateVariantAvailabilityAsync(input.ProductVariantId, newQty);
            existing.SetQuantity(newQty);
            await _cartItemRepository.UpdateAsync(existing);
        }
        else
        {
            var item = new CartItem(GuidGenerator.Create(), cart.Id, input.ProductVariantId, input.Quantity);
            await _cartItemRepository.InsertAsync(item);
        }

        await _catalog.ReserveAsync(input.ProductVariantId, input.Quantity);
        return await BuildCartDtoAsync(cart);
    }

    [AllowAnonymous]
    public async Task<CartDto> UpdateItemAsync(Guid cartItemId, UpdateCartItemDto input, Guid? guestCartId = null)
    {
        var cart = await ResolveOrCreateCartAsync(guestCartId);
        var item = await _cartItemRepository.FirstOrDefaultAsync(x => x.Id == cartItemId && x.CartId == cart.Id);
        if (item == null)
            throw new Volo.Abp.BusinessException("ECommerce:CartItemNotFound").WithData("CartItemId", cartItemId);

        await _catalog.ValidateVariantAvailabilityAsync(item.ProductVariantId, input.Quantity);
        var oldQty = item.Quantity;
        item.SetQuantity(input.Quantity);
        await _cartItemRepository.UpdateAsync(item);
        await _catalog.ReleaseAsync(item.ProductVariantId, oldQty);
        await _catalog.ReserveAsync(item.ProductVariantId, input.Quantity);
        return await BuildCartDtoAsync(cart);
    }

    [AllowAnonymous]
    public async Task<CartDto> RemoveItemAsync(Guid cartItemId, Guid? guestCartId = null)
    {
        var cart = await ResolveOrCreateCartAsync(guestCartId);
        var item = await _cartItemRepository.FirstOrDefaultAsync(x => x.Id == cartItemId && x.CartId == cart.Id);
        if (item != null)
        {
            await _catalog.ReleaseAsync(item.ProductVariantId, item.Quantity);
            await _cartItemRepository.DeleteAsync(item);
        }
        return await BuildCartDtoAsync(cart);
    }

    [AllowAnonymous]
    public async Task ClearCartAsync(Guid? guestCartId = null)
    {
        Cart cart;
        try
        {
            cart = await ResolveOrCreateCartAsync(guestCartId);
        }
        catch
        {
            return;
        }

        var items = await _cartItemRepository.GetListAsync(x => x.CartId == cart.Id);
        foreach (var item in items)
            await _catalog.ReleaseAsync(item.ProductVariantId, item.Quantity);
        if (items.Count > 0)
            await _cartItemRepository.DeleteManyAsync(items);
        await _cartRepository.DeleteAsync(cart);
    }

    [Authorize]
    public async Task<CartDto> MergeGuestCartAsync(Guid guestCartId)
    {
        var userId = CurrentUser.Id ?? throw new Volo.Abp.Authorization.AbpAuthorizationException("User must be logged in to merge cart.");
        var guestCart = await _cartRepository.FirstOrDefaultAsync(c => c.AnonymousId == guestCartId);
        if (guestCart == null)
            return await GetCartAsync(null);

        var userCart = await _cartRepository.FirstOrDefaultAsync(c => c.UserId == userId)
            ?? await CreateCartForUserAsync(userId);

        var guestItems = await _cartItemRepository.GetListAsync(x => x.CartId == guestCart.Id);
        foreach (var gi in guestItems)
        {
            var existingUser = await _cartItemRepository.FirstOrDefaultAsync(x => x.CartId == userCart.Id && x.ProductVariantId == gi.ProductVariantId);
            var newQty = (existingUser?.Quantity ?? 0) + gi.Quantity;
            await _catalog.ValidateVariantAvailabilityAsync(gi.ProductVariantId, newQty);
        }

        foreach (var gi in guestItems)
            await _catalog.ReleaseAsync(gi.ProductVariantId, gi.Quantity);

        foreach (var gi in guestItems)
        {
            var existing = await _cartItemRepository.FirstOrDefaultAsync(x =>
                x.CartId == userCart.Id && x.ProductVariantId == gi.ProductVariantId);
            if (existing != null)
            {
                existing.SetQuantity(existing.Quantity + gi.Quantity);
                await _cartItemRepository.UpdateAsync(existing);
            }
            else
            {
                await _cartItemRepository.InsertAsync(new CartItem(GuidGenerator.Create(), userCart.Id, gi.ProductVariantId, gi.Quantity));
            }
        }

        await _cartItemRepository.DeleteManyAsync(guestItems);
        await _cartRepository.DeleteAsync(guestCart);

        foreach (var gi in guestItems)
            await _catalog.ReserveAsync(gi.ProductVariantId, gi.Quantity);

        return await BuildCartDtoAsync(userCart);
    }

    private async Task<Cart> ResolveOrCreateCartAsync(Guid? guestCartId)
    {
        var userId = CurrentUser.Id;
        if (userId != null)
        {
            var userCart = await _cartRepository.FirstOrDefaultAsync(c => c.UserId == userId);
            if (userCart != null)
                return userCart;
            return await CreateCartForUserAsync(userId.Value);
        }

        if (guestCartId == null || guestCartId == Guid.Empty)
            throw new Volo.Abp.BusinessException("ECommerce:GuestCartIdRequired");

        var guestCart = await _cartRepository.FirstOrDefaultAsync(c => c.AnonymousId == guestCartId);
        if (guestCart != null)
            return guestCart;
        return await CreateCartForGuestAsync(guestCartId.Value);
    }

    private async Task<Cart> CreateCartForUserAsync(Guid userId)
    {
        var cart = new Cart(GuidGenerator.Create(), userId, null);
        await _cartRepository.InsertAsync(cart);
        return cart;
    }

    private async Task<Cart> CreateCartForGuestAsync(Guid anonymousId)
    {
        var cart = new Cart(GuidGenerator.Create(), null, anonymousId);
        await _cartRepository.InsertAsync(cart);
        return cart;
    }

    private async Task<CartDto> BuildCartDtoAsync(Cart cart)
    {
        var items = await _cartItemRepository.GetListAsync(x => x.CartId == cart.Id);
        if (items.Count == 0)
        {
            return new CartDto { Id = cart.Id, IsAuthenticated = cart.UserId != null, ItemCount = 0 };
        }

        var infos = await _catalog.GetVariantCartInfoAsync(items.Select(x => x.ProductVariantId));
        var infoMap = infos.ToDictionary(x => x.Id);

        var itemDtos = items.Select(item =>
        {
            infoMap.TryGetValue(item.ProductVariantId, out var info);
            return new CartItemDto
            {
                Id = item.Id,
                CartId = cart.Id,
                ProductVariantId = item.ProductVariantId,
                ProductId = info?.ProductId ?? Guid.Empty,
                ProductName = info?.ProductName ?? "",
                Sku = info?.Sku ?? "",
                UnitPrice = info?.UnitPrice,
                Quantity = item.Quantity,
                AvailableStock = info?.AvailableQuantity
            };
        }).ToList();

        return new CartDto
        {
            Id = cart.Id,
            IsAuthenticated = cart.UserId != null,
            Items = itemDtos,
            ItemCount = itemDtos.Sum(x => x.Quantity)
        };
    }
}
