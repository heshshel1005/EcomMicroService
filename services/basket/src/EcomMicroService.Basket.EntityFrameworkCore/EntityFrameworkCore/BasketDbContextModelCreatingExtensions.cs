using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace EcomMicroService.Basket.EntityFrameworkCore;

public static class BasketDbContextModelCreatingExtensions
{
    public static void ConfigureBasket(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<Cart>(b =>
        {
            b.ToTable(BasketDbProperties.DbTablePrefix + "Carts", BasketDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.TenantId, x.UserId }).IsUnique().HasFilter("\"UserId\" IS NOT NULL");
            b.HasIndex(x => new { x.TenantId, x.AnonymousId }).IsUnique().HasFilter("\"AnonymousId\" IS NOT NULL");
        });

        builder.Entity<CartItem>(b =>
        {
            b.ToTable(BasketDbProperties.DbTablePrefix + "CartItems", BasketDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.CartId);
            b.HasIndex(x => new { x.TenantId, x.CartId, x.ProductVariantId }).IsUnique();
        });
    }
}
