using EcomMicroService.Marketing.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace EcomMicroService.Marketing;

/* Domain tests are configured to use the EF Core provider.
 * You can switch to MongoDB, however your domain tests should be
 * database independent anyway.
 */
[DependsOn(typeof(MarketingEntityFrameworkCoreTestModule))]
public class MarketingDomainTestModule : AbpModule { }
