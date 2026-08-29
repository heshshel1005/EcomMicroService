using Volo.Abp.Autofac;
using Volo.Abp.Http.Client.IdentityModel;
using Volo.Abp.Modularity;

namespace EcomMicroService.Basket.HttpApi.Client.ConsoleTestApp;

[DependsOn(typeof(AbpAutofacModule))]
[DependsOn(typeof(BasketHttpApiClientModule))]
[DependsOn(typeof(AbpHttpClientIdentityModelModule))]
public class BasketConsoleApiClientModule : AbpModule { }
