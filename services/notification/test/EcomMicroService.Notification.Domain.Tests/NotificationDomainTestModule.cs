using EcomMicroService.Notification.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace EcomMicroService.Notification;

/* Domain tests are configured to use the EF Core provider.
 * You can switch to MongoDB, however your domain tests should be
 * database independent anyway.
 */
[DependsOn(typeof(NotificationEntityFrameworkCoreTestModule))]
public class NotificationDomainTestModule : AbpModule { }
