using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace EcomMicroService.Customer.EntityFrameworkCore;

[ConnectionStringName(EcomMicroServiceNames.CustomerDb)]
public class CustomerDbContext(DbContextOptions<CustomerDbContext> options)
    : AbpDbContext<CustomerDbContext>(options),
        ICustomerDbContext
{
    public DbSet<CustomerProfile> CustomerProfiles { get; set; }
    public DbSet<CustomerAddress> CustomerAddresses { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ConfigureCustomer();
    }
}
