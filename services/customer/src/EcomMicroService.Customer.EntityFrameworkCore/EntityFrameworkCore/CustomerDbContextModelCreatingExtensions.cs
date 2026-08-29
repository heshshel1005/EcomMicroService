using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace EcomMicroService.Customer.EntityFrameworkCore;

public static class CustomerDbContextModelCreatingExtensions
{
    public static void ConfigureCustomer(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<CustomerProfile>(b =>
        {
            b.ToTable(CustomerDbProperties.DbTablePrefix + "Profiles", CustomerDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.DisplayName).HasMaxLength(256);
            b.Property(x => x.PhoneNumber).HasMaxLength(32);
            b.HasIndex(x => new { x.TenantId, x.UserId }).IsUnique();
        });

        builder.Entity<CustomerAddress>(b =>
        {
            b.ToTable(CustomerDbProperties.DbTablePrefix + "Addresses", CustomerDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Label).HasMaxLength(64);
            b.Property(x => x.Street).HasMaxLength(512);
            b.HasIndex(x => x.UserId);
        });
    }
}
