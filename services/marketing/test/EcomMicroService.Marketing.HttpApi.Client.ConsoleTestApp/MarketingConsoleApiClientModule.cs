using Volo.Abp.Autofac;
using Volo.Abp.Http.Client.IdentityModel;
using Volo.Abp.Modularity;

namespace EcomMicroService.Marketing.HttpApi.Client.ConsoleTestApp;

[DependsOn(typeof(AbpAutofacModule))]
[DependsOn(typeof(MarketingHttpApiClientModule))]
[DependsOn(typeof(AbpHttpClientIdentityModelModule))]
public class MarketingConsoleApiClientModule : AbpModule { }
