using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using EcomMicroService.Ordering.Orders;

namespace EcomMicroService.Ordering.EntityFrameworkCore;

[ConnectionStringName(EcomMicroServiceNames.OrderingDb)]
public class OrderingDbContext(DbContextOptions<OrderingDbContext> options)
    : AbpDbContext<OrderingDbContext>(options),
        IOrderingDbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderLine> OrderLines { get; set; }
    public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }
    public DbSet<Shipment> Shipments { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ConfigureOrdering();
    }
}
