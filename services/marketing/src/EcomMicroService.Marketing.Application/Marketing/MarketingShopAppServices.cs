using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace EcomMicroService.Marketing;

[AllowAnonymous]
public class CouponShopAppService : ApplicationService, ICouponShopAppService
{
    private readonly IRepository<Coupon, Guid> _coupons;
    private readonly IRepository<CouponUsage, Guid> _usages;

    public CouponShopAppService(IRepository<Coupon, Guid> coupons, IRepository<CouponUsage, Guid> usages)
    {
        _coupons = coupons;
        _usages = usages;
    }

    public async Task<CouponPreviewDto> PreviewAsync(string code, decimal subtotal)
    {
        var coupon = await _coupons.FirstOrDefaultAsync(c => c.Code == (code ?? "").Trim().ToUpperInvariant());
        if (coupon == null || !coupon.IsValidAt(DateTime.UtcNow) || subtotal < coupon.MinOrderAmount)
            return new CouponPreviewDto { IsValid = false };
        var totalUsages = await _usages.CountAsync(x => x.CouponId == coupon.Id);
        if (coupon.TotalUsageLimit.HasValue && totalUsages >= coupon.TotalUsageLimit.Value)
            return new CouponPreviewDto { IsValid = false };
        return new CouponPreviewDto
        {
            Id = coupon.Id,
            Code = coupon.Code,
            DiscountAmount = coupon.CalculateDiscount(subtotal),
            IsValid = true
        };
    }

    public async Task RecordUsageAsync(RecordCouponUsageDto input)
    {
        await _usages.InsertAsync(new CouponUsage(GuidGenerator.Create(), input.CouponId, input.UserId, input.OrderId));
    }
}

[Authorize("ECommerce.Administration")]
public class CouponAdminAppService : ApplicationService, ICouponAdminAppService
{
    private readonly IRepository<Coupon, Guid> _coupons;

    public CouponAdminAppService(IRepository<Coupon, Guid> coupons) => _coupons = coupons;

    public async Task<CouponDto> CreateAsync(CreateCouponDto input)
    {
        var entity = new Coupon(GuidGenerator.Create(), input.Code, input.Type, input.Value, input.MinOrderAmount,
            input.ValidFrom, input.ValidTo, input.TotalUsageLimit, input.PerUserUsageLimit)
        { IsActive = input.IsActive };
        await _coupons.InsertAsync(entity);
        return Map(entity);
    }

    public async Task<PagedResultDto<CouponDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var query = await _coupons.GetQueryableAsync();
        var total = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query.OrderByDescending(c => c.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount > 0 ? input.MaxResultCount : 10));
        return new PagedResultDto<CouponDto>(total, items.Select(Map).ToList());
    }

    private static CouponDto Map(Coupon c) => new()
    {
        Id = c.Id,
        Code = c.Code,
        Type = (int)c.Type,
        Value = c.Value,
        MinOrderAmount = c.MinOrderAmount,
        ValidFrom = c.ValidFrom,
        ValidTo = c.ValidTo,
        TotalUsageLimit = c.TotalUsageLimit,
        PerUserUsageLimit = c.PerUserUsageLimit,
        IsActive = c.IsActive
    };
}

[Authorize]
public class WishlistAppService : ApplicationService, IWishlistAppService
{
    private readonly IRepository<Wishlist, Guid> _wishlists;
    private readonly IRepository<WishlistItem, Guid> _items;

    public WishlistAppService(IRepository<Wishlist, Guid> wishlists, IRepository<WishlistItem, Guid> items)
    {
        _wishlists = wishlists;
        _items = items;
    }

    public async Task<WishlistDto> GetListAsync() => await BuildAsync(await GetOrCreateAsync());

    public async Task<WishlistDto> AddItemAsync(Guid productVariantId)
    {
        var list = await GetOrCreateAsync();
        var existing = await _items.FirstOrDefaultAsync(i => i.WishlistId == list.Id && i.ProductVariantId == productVariantId);
        if (existing == null)
            await _items.InsertAsync(new WishlistItem(GuidGenerator.Create(), list.Id, productVariantId));
        return await BuildAsync(list);
    }

    public async Task<WishlistDto> RemoveItemAsync(Guid wishlistItemId)
    {
        var list = await GetOrCreateAsync();
        var item = await _items.GetAsync(wishlistItemId);
        if (item.WishlistId == list.Id)
            await _items.DeleteAsync(item);
        return await BuildAsync(list);
    }

    public Task AddToCartAsync(Guid wishlistItemId) => Task.CompletedTask;

    private async Task<Wishlist> GetOrCreateAsync()
    {
        var userId = CurrentUser.Id ?? throw new Volo.Abp.Authorization.AbpAuthorizationException();
        var list = await _wishlists.FirstOrDefaultAsync(w => w.UserId == userId);
        if (list != null) return list;
        list = new Wishlist(GuidGenerator.Create(), userId);
        await _wishlists.InsertAsync(list);
        return list;
    }

    private async Task<WishlistDto> BuildAsync(Wishlist list)
    {
        var items = await _items.GetListAsync(i => i.WishlistId == list.Id);
        return new WishlistDto
        {
            Id = list.Id,
            Items = items.Select(i => new WishlistItemDto { Id = i.Id, ProductVariantId = i.ProductVariantId }).ToList()
        };
    }
}

[AllowAnonymous]
public class NewsletterSubscriberAppService : ApplicationService, INewsletterSubscriberAppService
{
    private readonly IRepository<NewsletterSubscriber, Guid> _subscribers;

    public NewsletterSubscriberAppService(IRepository<NewsletterSubscriber, Guid> subscribers) => _subscribers = subscribers;

    [Authorize]
    public async Task<NewsletterSubscriptionStatusDto> GetMyStatusAsync()
    {
        var email = CurrentUser.Email ?? "";
        var sub = await _subscribers.FirstOrDefaultAsync(s => s.Email == email.ToLowerInvariant());
        return new NewsletterSubscriptionStatusDto { Email = email, IsSubscribed = sub?.IsActive == true };
    }

    public async Task SubscribeAsync(SubscribeNewsletterDto input)
    {
        var email = (input.Email ?? CurrentUser.Email ?? "").Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email)) return;
        var existing = await _subscribers.FirstOrDefaultAsync(s => s.Email == email);
        if (existing == null)
            await _subscribers.InsertAsync(new NewsletterSubscriber(GuidGenerator.Create(), email, input.Name));
        else
        {
            existing.Resubscribe();
            await _subscribers.UpdateAsync(existing);
        }
    }

    public async Task UnsubscribeAsync(string? email = null)
    {
        var target = (email ?? CurrentUser.Email ?? "").Trim().ToLowerInvariant();
        var existing = await _subscribers.FirstOrDefaultAsync(s => s.Email == target);
        if (existing == null) return;
        existing.Unsubscribe();
        await _subscribers.UpdateAsync(existing);
    }
}

[Authorize("ECommerce.Administration")]
public class NewsletterSubscriberAdminAppService : ApplicationService, INewsletterSubscriberAdminAppService
{
    private readonly IRepository<NewsletterSubscriber, Guid> _subscribers;
    public NewsletterSubscriberAdminAppService(IRepository<NewsletterSubscriber, Guid> subscribers) => _subscribers = subscribers;

    public async Task<PagedResultDto<NewsletterSubscriberDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var query = await _subscribers.GetQueryableAsync();
        var total = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query.OrderByDescending(s => s.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount > 0 ? input.MaxResultCount : 20));
        return new PagedResultDto<NewsletterSubscriberDto>(total, items.Select(s => new NewsletterSubscriberDto
        {
            Id = s.Id, Email = s.Email, Name = s.Name, IsActive = s.IsActive
        }).ToList());
    }
}

public class LoyaltyPointsAppService : ApplicationService, ILoyaltyPointsAppService
{
    private readonly IRepository<CustomerPoints, Guid> _points;
    private readonly IRepository<PointsTransaction, Guid> _tx;

    public LoyaltyPointsAppService(IRepository<CustomerPoints, Guid> points, IRepository<PointsTransaction, Guid> tx)
    {
        _points = points;
        _tx = tx;
    }

    [Authorize]
    public async Task<CustomerPointsDto> GetMyPointsAsync()
    {
        var userId = CurrentUser.Id ?? throw new Volo.Abp.Authorization.AbpAuthorizationException();
        var rec = await _points.FirstOrDefaultAsync(p => p.UserId == userId);
        return new CustomerPointsDto { Balance = rec?.Balance ?? 0, Tier = rec?.Tier };
    }

    [AllowAnonymous]
    public async Task AwardForOrderAsync(AwardLoyaltyDto input)
    {
        if (input.UserId == null || input.OrderTotal <= 0) return;
        var points = (int)Math.Floor(input.OrderTotal);
        var rec = await _points.FirstOrDefaultAsync(p => p.UserId == input.UserId.Value);
        if (rec == null)
        {
            rec = new CustomerPoints(GuidGenerator.Create(), input.UserId.Value, points);
            await _points.InsertAsync(rec);
        }
        else
        {
            rec.AddPoints(points);
            await _points.UpdateAsync(rec);
        }
        await _tx.InsertAsync(new PointsTransaction(GuidGenerator.Create(), input.UserId.Value, points, PointsTransactionType.Earn, input.OrderId, null, "Order"));
    }
}

[Authorize]
public class GiftRegistryAppService : ApplicationService, IGiftRegistryAppService
{
    private readonly IRepository<GiftRegistry, Guid> _registries;
    private readonly IRepository<GiftRegistryItem, Guid> _items;
    private readonly IRepository<GiftRegistryClaim, Guid> _claims;

    public GiftRegistryAppService(
        IRepository<GiftRegistry, Guid> registries,
        IRepository<GiftRegistryItem, Guid> items,
        IRepository<GiftRegistryClaim, Guid> claims)
    {
        _registries = registries;
        _items = items;
        _claims = claims;
    }

    [AllowAnonymous]
    public async Task<GiftRegistryDto?> GetBySlugAsync(string slug)
    {
        var reg = await _registries.FirstOrDefaultAsync(r => r.Slug == slug.Trim().ToLowerInvariant());
        return reg == null ? null : await MapAsync(reg);
    }

    [AllowAnonymous]
    public async Task ClaimAsync(ClaimRegistryItemDto input)
    {
        var item = await _items.GetAsync(input.GiftRegistryItemId);
        var qty = Math.Min(input.Quantity, item.QuantityRemaining);
        if (qty <= 0) return;
        item.QuantityClaimed += qty;
        await _items.UpdateAsync(item);
        await _claims.InsertAsync(new GiftRegistryClaim(GuidGenerator.Create(), item.Id, qty, CurrentUser.Id, input.ClaimantName, input.Message));
    }

    public async Task<GiftRegistryDto> CreateAsync(CreateGiftRegistryDto input)
    {
        var userId = CurrentUser.Id ?? throw new Volo.Abp.Authorization.AbpAuthorizationException();
        var entity = new GiftRegistry(GuidGenerator.Create(), userId, input.Title, input.Slug, input.EventDate);
        await _registries.InsertAsync(entity);
        return await MapAsync(entity);
    }

    public async Task<List<GiftRegistryDto>> GetMyRegistriesAsync()
    {
        var userId = CurrentUser.Id ?? throw new Volo.Abp.Authorization.AbpAuthorizationException();
        var list = await _registries.GetListAsync(r => r.OwnerUserId == userId);
        var result = new List<GiftRegistryDto>();
        foreach (var r in list) result.Add(await MapAsync(r));
        return result;
    }

    public async Task<GiftRegistryDto> AddItemAsync(Guid giftRegistryId, AddGiftRegistryItemDto input)
    {
        var reg = await _registries.GetAsync(giftRegistryId);
        EnsureOwner(reg);
        await _items.InsertAsync(new GiftRegistryItem(GuidGenerator.Create(), giftRegistryId, input.ProductVariantId, input.DesiredQuantity, input.Note));
        return await MapAsync(reg);
    }

    public async Task<GiftRegistryDto> RemoveItemAsync(Guid giftRegistryId, Guid giftRegistryItemId)
    {
        var reg = await _registries.GetAsync(giftRegistryId);
        EnsureOwner(reg);
        await _items.DeleteAsync(giftRegistryItemId);
        return await MapAsync(reg);
    }

    private void EnsureOwner(GiftRegistry reg)
    {
        if (reg.OwnerUserId != CurrentUser.Id)
            throw new Volo.Abp.Authorization.AbpAuthorizationException();
    }

    private async Task<GiftRegistryDto> MapAsync(GiftRegistry reg)
    {
        var items = await _items.GetListAsync(i => i.GiftRegistryId == reg.Id);
        return new GiftRegistryDto
        {
            Id = reg.Id,
            Title = reg.Title,
            Slug = reg.Slug,
            EventDate = reg.EventDate,
            Items = items.Select(i => new GiftRegistryItemDto
            {
                Id = i.Id,
                ProductVariantId = i.ProductVariantId,
                DesiredQuantity = i.DesiredQuantity,
                QuantityClaimed = i.QuantityClaimed,
                QuantityRemaining = i.QuantityRemaining,
                Note = i.Note
            }).ToList()
        };
    }
}
