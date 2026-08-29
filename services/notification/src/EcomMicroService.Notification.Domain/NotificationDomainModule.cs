using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace EcomMicroService.Notification;

[DependsOn(typeof(AbpDddDomainModule))]
[DependsOn(typeof(NotificationDomainSharedModule))]
public class NotificationDomainModule : AbpModule { }
