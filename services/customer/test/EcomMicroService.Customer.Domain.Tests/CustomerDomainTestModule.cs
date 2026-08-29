using EcomMicroService.Customer.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace EcomMicroService.Customer;

/* Domain tests are configured to use the EF Core provider.
 * You can switch to MongoDB, however your domain tests should be
 * database independent anyway.
 */
[DependsOn(typeof(CustomerEntityFrameworkCoreTestModule))]
public class CustomerDomainTestModule : AbpModule { }
