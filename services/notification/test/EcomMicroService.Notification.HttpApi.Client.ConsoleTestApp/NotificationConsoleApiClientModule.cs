using Volo.Abp.Autofac;
using Volo.Abp.Http.Client.IdentityModel;
using Volo.Abp.Modularity;

namespace EcomMicroService.Notification.HttpApi.Client.ConsoleTestApp;

[DependsOn(typeof(AbpAutofacModule))]
[DependsOn(typeof(NotificationHttpApiClientModule))]
[DependsOn(typeof(AbpHttpClientIdentityModelModule))]
public class NotificationConsoleApiClientModule : AbpModule { }
