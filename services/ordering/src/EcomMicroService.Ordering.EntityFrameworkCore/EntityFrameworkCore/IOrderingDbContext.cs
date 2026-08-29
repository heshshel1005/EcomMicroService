using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using EcomMicroService.Ordering.Orders;

namespace EcomMicroService.Ordering.EntityFrameworkCore;

[ConnectionStringName(EcomMicroServiceNames.OrderingDb)]
public interface IOrderingDbContext : IEfCoreDbContext
{
    DbSet<Order> Orders { get; }
    DbSet<OrderLine> OrderLines { get; }
    DbSet<OrderStatusHistory> OrderStatusHistories { get; }
    DbSet<Shipment> Shipments { get; }
}
