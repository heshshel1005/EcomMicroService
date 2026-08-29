using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace EcomMicroService.Customer.EntityFrameworkCore;

[ConnectionStringName(EcomMicroServiceNames.CustomerDb)]
public interface ICustomerDbContext : IEfCoreDbContext
{
    DbSet<CustomerProfile> CustomerProfiles { get; }
    DbSet<CustomerAddress> CustomerAddresses { get; }
}
