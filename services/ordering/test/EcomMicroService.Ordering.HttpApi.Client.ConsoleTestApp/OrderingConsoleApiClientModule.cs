using Volo.Abp.Autofac;
using Volo.Abp.Http.Client.IdentityModel;
using Volo.Abp.Modularity;

namespace EcomMicroService.Ordering.HttpApi.Client.ConsoleTestApp;

[DependsOn(typeof(AbpAutofacModule))]
[DependsOn(typeof(OrderingHttpApiClientModule))]
[DependsOn(typeof(AbpHttpClientIdentityModelModule))]
public class OrderingConsoleApiClientModule : AbpModule { }
