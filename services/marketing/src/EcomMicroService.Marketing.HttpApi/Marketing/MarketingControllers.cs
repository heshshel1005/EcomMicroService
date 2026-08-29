using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

namespace EcomMicroService.Marketing;

[RemoteService(Name = "Marketing")]
[Area("marketing")]
[Route("api/marketing/coupons")]
public class CouponController : MarketingController
{
    private readonly ICouponShopAppService _shop;
    private readonly ICouponAdminAppService _admin;

    public CouponController(ICouponShopAppService shop, ICouponAdminAppService admin)
    {
        _shop = shop;
        _admin = admin;
    }

    [HttpGet("preview")]
    public Task<CouponPreviewDto> PreviewAsync([FromQuery] string code, [FromQuery] decimal subtotal) => _shop.PreviewAsync(code, subtotal);

    [HttpPost("record-usage")]
    public Task RecordUsageAsync([FromBody] RecordCouponUsageDto input) => _shop.RecordUsageAsync(input);

    [HttpPost]
    public Task<CouponDto> CreateAsync([FromBody] CreateCouponDto input) => _admin.CreateAsync(input);

    [HttpGet]
    public Task<PagedResultDto<CouponDto>> GetListAsync([FromQuery] PagedAndSortedResultRequestDto input) => _admin.GetListAsync(input);
}

[RemoteService(Name = "Marketing")]
[Area("marketing")]
[Route("api/marketing/wishlist")]
public class WishlistController : MarketingController
{
    private readonly IWishlistAppService _app;
    public WishlistController(IWishlistAppService app) => _app = app;
    [HttpGet] public Task<WishlistDto> GetListAsync() => _app.GetListAsync();
    [HttpPost("items")] public Task<WishlistDto> AddItemAsync([FromQuery] Guid productVariantId) => _app.AddItemAsync(productVariantId);
    [HttpDelete("items/{wishlistItemId}")] public Task<WishlistDto> RemoveItemAsync(Guid wishlistItemId) => _app.RemoveItemAsync(wishlistItemId);
    [HttpPost("items/{wishlistItemId}/add-to-cart")] public Task AddToCartAsync(Guid wishlistItemId) => _app.AddToCartAsync(wishlistItemId);
}

[RemoteService(Name = "Marketing")]
[Area("marketing")]
[Route("api/marketing/newsletter")]
public class NewsletterController : MarketingController
{
    private readonly INewsletterSubscriberAppService _app;
    private readonly INewsletterSubscriberAdminAppService _admin;
    public NewsletterController(INewsletterSubscriberAppService app, INewsletterSubscriberAdminAppService admin)
    {
        _app = app;
        _admin = admin;
    }
    [HttpGet("my-status")] public Task<NewsletterSubscriptionStatusDto> GetMyStatusAsync() => _app.GetMyStatusAsync();
    [HttpPost("subscribe")] public Task SubscribeAsync([FromBody] SubscribeNewsletterDto input) => _app.SubscribeAsync(input);
    [HttpPost("unsubscribe")] public Task UnsubscribeAsync([FromQuery] string? email = null) => _app.UnsubscribeAsync(email);
    [HttpGet("admin")] public Task<PagedResultDto<NewsletterSubscriberDto>> AdminListAsync([FromQuery] PagedAndSortedResultRequestDto input) => _admin.GetListAsync(input);
}

[RemoteService(Name = "Marketing")]
[Area("marketing")]
[Route("api/marketing/loyalty")]
public class LoyaltyController : MarketingController
{
    private readonly ILoyaltyPointsAppService _app;
    public LoyaltyController(ILoyaltyPointsAppService app) => _app = app;
    [HttpGet("me")] public Task<CustomerPointsDto> GetMyPointsAsync() => _app.GetMyPointsAsync();
    [HttpPost("award-order")] public Task AwardForOrderAsync([FromBody] AwardLoyaltyDto input) => _app.AwardForOrderAsync(input);
}

[RemoteService(Name = "Marketing")]
[Area("marketing")]
[Route("api/marketing/gift-registry")]
public class GiftRegistryController : MarketingController
{
    private readonly IGiftRegistryAppService _app;
    public GiftRegistryController(IGiftRegistryAppService app) => _app = app;
    [HttpGet("by-slug/{slug}")] public Task<GiftRegistryDto?> GetBySlugAsync(string slug) => _app.GetBySlugAsync(slug);
    [HttpPost("claim")] public Task ClaimAsync([FromBody] ClaimRegistryItemDto input) => _app.ClaimAsync(input);
    [HttpPost] public Task<GiftRegistryDto> CreateAsync([FromBody] CreateGiftRegistryDto input) => _app.CreateAsync(input);
    [HttpGet("mine")] public Task<List<GiftRegistryDto>> GetMyRegistriesAsync() => _app.GetMyRegistriesAsync();
    [HttpPost("{id}/items")] public Task<GiftRegistryDto> AddItemAsync(Guid id, [FromBody] AddGiftRegistryItemDto input) => _app.AddItemAsync(id, input);
    [HttpDelete("{id}/items/{itemId}")] public Task<GiftRegistryDto> RemoveItemAsync(Guid id, Guid itemId) => _app.RemoveItemAsync(id, itemId);
}
