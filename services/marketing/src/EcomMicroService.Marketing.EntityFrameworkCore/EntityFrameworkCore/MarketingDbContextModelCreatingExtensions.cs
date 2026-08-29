using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace EcomMicroService.Marketing.EntityFrameworkCore;

public static class MarketingDbContextModelCreatingExtensions
{
    public static void ConfigureMarketing(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));
        builder.Entity<Coupon>(b =>
        {
            b.ToTable(MarketingDbProperties.DbTablePrefix + "Coupons", MarketingDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Code).IsRequired().HasMaxLength(64);
            b.HasIndex(x => x.Code).IsUnique();
        });
        builder.Entity<CouponUsage>(b =>
        {
            b.ToTable(MarketingDbProperties.DbTablePrefix + "CouponUsages", MarketingDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.CouponId);
        });
        builder.Entity<Wishlist>(b =>
        {
            b.ToTable(MarketingDbProperties.DbTablePrefix + "Wishlists", MarketingDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.UserId);
        });
        builder.Entity<WishlistItem>(b =>
        {
            b.ToTable(MarketingDbProperties.DbTablePrefix + "WishlistItems", MarketingDbProperties.DbSchema);
            b.ConfigureByConvention();
        });
        builder.Entity<NewsletterSubscriber>(b =>
        {
            b.ToTable(MarketingDbProperties.DbTablePrefix + "NewsletterSubscribers", MarketingDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Email).IsRequired().HasMaxLength(256);
            b.HasIndex(x => x.Email);
        });
        builder.Entity<CustomerPoints>(b =>
        {
            b.ToTable(MarketingDbProperties.DbTablePrefix + "CustomerPoints", MarketingDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.UserId).IsUnique();
        });
        builder.Entity<PointsTransaction>(b =>
        {
            b.ToTable(MarketingDbProperties.DbTablePrefix + "PointsTransactions", MarketingDbProperties.DbSchema);
            b.ConfigureByConvention();
        });
        builder.Entity<GiftRegistry>(b =>
        {
            b.ToTable(MarketingDbProperties.DbTablePrefix + "GiftRegistries", MarketingDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.Slug);
        });
        builder.Entity<GiftRegistryItem>(b =>
        {
            b.ToTable(MarketingDbProperties.DbTablePrefix + "GiftRegistryItems", MarketingDbProperties.DbSchema);
            b.ConfigureByConvention();
        });
        builder.Entity<GiftRegistryClaim>(b =>
        {
            b.ToTable(MarketingDbProperties.DbTablePrefix + "GiftRegistryClaims", MarketingDbProperties.DbSchema);
            b.ConfigureByConvention();
        });
        builder.Entity<RedemptionRule>(b =>
        {
            b.ToTable(MarketingDbProperties.DbTablePrefix + "RedemptionRules", MarketingDbProperties.DbSchema);
            b.ConfigureByConvention();
        });
    }
}
