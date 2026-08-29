using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EcomMicroService.Marketing;

public class CouponPreviewDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public bool IsValid { get; set; }
}

public class RecordCouponUsageDto
{
    public Guid CouponId { get; set; }
    public Guid? UserId { get; set; }
    public Guid OrderId { get; set; }
}

public class AwardLoyaltyDto
{
    public Guid OrderId { get; set; }
    public Guid? UserId { get; set; }
    public decimal OrderTotal { get; set; }
}

public class CreateCouponDto
{
    [Required]
    [StringLength(64)]
    public string Code { get; set; } = string.Empty;
    public CouponType Type { get; set; }
    public decimal Value { get; set; }
    public decimal MinOrderAmount { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public int? TotalUsageLimit { get; set; }
    public int? PerUserUsageLimit { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CouponDto : EntityDto<Guid>
{
    public string Code { get; set; } = string.Empty;
    public int Type { get; set; }
    public decimal Value { get; set; }
    public decimal MinOrderAmount { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public int? TotalUsageLimit { get; set; }
    public int? PerUserUsageLimit { get; set; }
    public bool IsActive { get; set; }
}

public class WishlistItemDto
{
    public Guid Id { get; set; }
    public Guid ProductVariantId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public int? AvailableQuantity { get; set; }
}

public class WishlistDto
{
    public Guid Id { get; set; }
    public List<WishlistItemDto> Items { get; set; } = new();
}

public class SubscribeNewsletterDto
{
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
}

public class NewsletterSubscriptionStatusDto
{
    public bool IsSubscribed { get; set; }
    public string Email { get; set; } = string.Empty;
}

public class NewsletterSubscriberDto : EntityDto<Guid>
{
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public bool IsActive { get; set; }
}

public class CustomerPointsDto
{
    public int Balance { get; set; }
    public string? Tier { get; set; }
}

public class CreateGiftRegistryDto
{
    [Required]
    [StringLength(256)]
    public string Title { get; set; } = string.Empty;
    [Required]
    [StringLength(256)]
    public string Slug { get; set; } = string.Empty;
    public DateTime? EventDate { get; set; }
}

public class AddGiftRegistryItemDto
{
    public Guid ProductVariantId { get; set; }
    public int DesiredQuantity { get; set; } = 1;
    public string? Note { get; set; }
}

public class ClaimRegistryItemDto
{
    public Guid GiftRegistryItemId { get; set; }
    public int Quantity { get; set; } = 1;
    public string? ClaimantName { get; set; }
    public string? Message { get; set; }
}

public class GiftRegistryItemDto
{
    public Guid Id { get; set; }
    public Guid ProductVariantId { get; set; }
    public int DesiredQuantity { get; set; }
    public int QuantityClaimed { get; set; }
    public int QuantityRemaining { get; set; }
    public string? Note { get; set; }
}

public class GiftRegistryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public DateTime? EventDate { get; set; }
    public List<GiftRegistryItemDto> Items { get; set; } = new();
}

[Volo.Abp.RemoteService(IsEnabled = false)]
public interface ICouponShopAppService : IApplicationService
{
    Task<CouponPreviewDto> PreviewAsync(string code, decimal subtotal);
    Task RecordUsageAsync(RecordCouponUsageDto input);
}

[Volo.Abp.RemoteService(IsEnabled = false)]
public interface ICouponAdminAppService : IApplicationService
{
    Task<CouponDto> CreateAsync(CreateCouponDto input);
    Task<PagedResultDto<CouponDto>> GetListAsync(PagedAndSortedResultRequestDto input);
}

[Volo.Abp.RemoteService(IsEnabled = false)]
public interface IWishlistAppService : IApplicationService
{
    Task<WishlistDto> GetListAsync();
    Task<WishlistDto> AddItemAsync(Guid productVariantId);
    Task<WishlistDto> RemoveItemAsync(Guid wishlistItemId);
    Task AddToCartAsync(Guid wishlistItemId);
}

[Volo.Abp.RemoteService(IsEnabled = false)]
public interface INewsletterSubscriberAppService : IApplicationService
{
    Task<NewsletterSubscriptionStatusDto> GetMyStatusAsync();
    Task SubscribeAsync(SubscribeNewsletterDto input);
    Task UnsubscribeAsync(string? email = null);
}

[Volo.Abp.RemoteService(IsEnabled = false)]
public interface INewsletterSubscriberAdminAppService : IApplicationService
{
    Task<PagedResultDto<NewsletterSubscriberDto>> GetListAsync(PagedAndSortedResultRequestDto input);
}

[Volo.Abp.RemoteService(IsEnabled = false)]
public interface ILoyaltyPointsAppService : IApplicationService
{
    Task<CustomerPointsDto> GetMyPointsAsync();
    Task AwardForOrderAsync(AwardLoyaltyDto input);
}

[Volo.Abp.RemoteService(IsEnabled = false)]
public interface IGiftRegistryAppService : IApplicationService
{
    Task<GiftRegistryDto?> GetBySlugAsync(string slug);
    Task ClaimAsync(ClaimRegistryItemDto input);
    Task<GiftRegistryDto> CreateAsync(CreateGiftRegistryDto input);
    Task<List<GiftRegistryDto>> GetMyRegistriesAsync();
    Task<GiftRegistryDto> AddItemAsync(Guid giftRegistryId, AddGiftRegistryItemDto input);
    Task<GiftRegistryDto> RemoveItemAsync(Guid giftRegistryId, Guid giftRegistryItemId);
}
