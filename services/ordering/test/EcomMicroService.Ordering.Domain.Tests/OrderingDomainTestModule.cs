using EcomMicroService.Ordering.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace EcomMicroService.Ordering;

/* Domain tests are configured to use the EF Core provider.
 * You can switch to MongoDB, however your domain tests should be
 * database independent anyway.
 */
[DependsOn(typeof(OrderingEntityFrameworkCoreTestModule))]
public class OrderingDomainTestModule : AbpModule { }
