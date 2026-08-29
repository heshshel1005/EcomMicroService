using Volo.Abp.Modularity;

namespace EcomMicroService.Notification;

[DependsOn(typeof(NotificationApplicationModule))]
[DependsOn(typeof(NotificationDomainTestModule))]
public class NotificationApplicationTestModule : AbpModule { }
