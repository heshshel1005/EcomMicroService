using Volo.Abp.Autofac;
using Volo.Abp.Http.Client.IdentityModel;
using Volo.Abp.Modularity;

namespace EcomMicroService.Customer.HttpApi.Client.ConsoleTestApp;

[DependsOn(typeof(AbpAutofacModule))]
[DependsOn(typeof(CustomerHttpApiClientModule))]
[DependsOn(typeof(AbpHttpClientIdentityModelModule))]
public class CustomerConsoleApiClientModule : AbpModule { }
