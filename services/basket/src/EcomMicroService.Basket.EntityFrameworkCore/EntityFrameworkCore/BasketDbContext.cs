using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace EcomMicroService.Basket.EntityFrameworkCore;

[ConnectionStringName(EcomMicroServiceNames.BasketDb)]
public class BasketDbContext(DbContextOptions<BasketDbContext> options)
    : AbpDbContext<BasketDbContext>(options),
        IBasketDbContext
{
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ConfigureBasket();
    }
}
