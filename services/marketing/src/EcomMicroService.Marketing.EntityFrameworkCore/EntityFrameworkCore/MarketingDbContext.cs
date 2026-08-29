using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace EcomMicroService.Marketing.EntityFrameworkCore;

[ConnectionStringName(EcomMicroServiceNames.MarketingDb)]
public class MarketingDbContext(DbContextOptions<MarketingDbContext> options)
    : AbpDbContext<MarketingDbContext>(options),
        IMarketingDbContext
{
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<CouponUsage> CouponUsages { get; set; }
    public DbSet<Wishlist> Wishlists { get; set; }
    public DbSet<WishlistItem> WishlistItems { get; set; }
    public DbSet<NewsletterSubscriber> NewsletterSubscribers { get; set; }
    public DbSet<CustomerPoints> CustomerPoints { get; set; }
    public DbSet<PointsTransaction> PointsTransactions { get; set; }
    public DbSet<GiftRegistry> GiftRegistries { get; set; }
    public DbSet<GiftRegistryItem> GiftRegistryItems { get; set; }
    public DbSet<GiftRegistryClaim> GiftRegistryClaims { get; set; }
    public DbSet<RedemptionRule> RedemptionRules { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ConfigureMarketing();
    }
}
