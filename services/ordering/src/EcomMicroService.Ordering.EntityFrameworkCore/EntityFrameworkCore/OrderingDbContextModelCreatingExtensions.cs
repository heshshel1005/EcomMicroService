using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;
using EcomMicroService.Ordering.Orders;

namespace EcomMicroService.Ordering.EntityFrameworkCore;

public static class OrderingDbContextModelCreatingExtensions
{
    public static void ConfigureOrdering(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<Order>(b =>
        {
            b.ToTable(OrderingDbProperties.DbTablePrefix + "Orders", OrderingDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ContactEmail).HasMaxLength(256);
            b.Property(x => x.ContactPhone).HasMaxLength(32);
            b.Property(x => x.ContactName).HasMaxLength(256);
            b.Property(x => x.ShippingStreet).HasMaxLength(512);
            b.Property(x => x.SubTotal).HasPrecision(18, 2);
            b.Property(x => x.ShippingAmount).HasPrecision(18, 2);
            b.Property(x => x.TaxAmount).HasPrecision(18, 2);
            b.Property(x => x.Total).HasPrecision(18, 2);
            b.Property(x => x.DiscountAmount).HasPrecision(18, 2);
            b.HasIndex(x => x.UserId);
            b.HasIndex(x => x.Status);
            b.HasMany(o => o.Lines).WithOne().HasForeignKey(l => l.OrderId).IsRequired();
        });

        builder.Entity<OrderLine>(b =>
        {
            b.ToTable(OrderingDbProperties.DbTablePrefix + "OrderLines", OrderingDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ProductName).HasMaxLength(256);
            b.Property(x => x.Sku).HasMaxLength(64);
            b.Property(x => x.UnitPrice).HasPrecision(18, 2);
            b.HasIndex(x => x.OrderId);
        });

        builder.Entity<OrderStatusHistory>(b =>
        {
            b.ToTable(OrderingDbProperties.DbTablePrefix + "OrderStatusHistories", OrderingDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.OrderId);
        });

        builder.Entity<Shipment>(b =>
        {
            b.ToTable(OrderingDbProperties.DbTablePrefix + "Shipments", OrderingDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Carrier).HasMaxLength(128);
            b.Property(x => x.TrackingNumber).HasMaxLength(128);
            b.HasIndex(x => x.OrderId);
        });
    }
}
