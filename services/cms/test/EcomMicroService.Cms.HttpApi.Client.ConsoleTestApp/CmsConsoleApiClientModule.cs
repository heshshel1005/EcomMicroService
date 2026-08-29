using Volo.Abp.Autofac;
using Volo.Abp.Http.Client.IdentityModel;
using Volo.Abp.Modularity;

namespace EcomMicroService.Cms.HttpApi.Client.ConsoleTestApp;

[DependsOn(typeof(AbpAutofacModule))]
[DependsOn(typeof(CmsHttpApiClientModule))]
[DependsOn(typeof(AbpHttpClientIdentityModelModule))]
public class CmsConsoleApiClientModule : AbpModule { }
