using Volo.Abp.Autofac;
using Volo.Abp.Http.Client.IdentityModel;
using Volo.Abp.Modularity;

namespace EcomMicroService.Catalog.HttpApi.Client.ConsoleTestApp;

[DependsOn(typeof(AbpAutofacModule))]
[DependsOn(typeof(CatalogHttpApiClientModule))]
[DependsOn(typeof(AbpHttpClientIdentityModelModule))]
public class CatalogConsoleApiClientModule : AbpModule { }
