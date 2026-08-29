using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace EcomMicroService.Basket.EntityFrameworkCore;

[ConnectionStringName(EcomMicroServiceNames.BasketDb)]
public interface IBasketDbContext : IEfCoreDbContext
{
    DbSet<Cart> Carts { get; }
    DbSet<CartItem> CartItems { get; }
}
