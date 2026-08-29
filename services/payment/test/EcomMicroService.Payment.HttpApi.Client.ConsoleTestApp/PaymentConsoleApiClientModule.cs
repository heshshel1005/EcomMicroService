using Volo.Abp.Autofac;
using Volo.Abp.Http.Client.IdentityModel;
using Volo.Abp.Modularity;

namespace EcomMicroService.Payment.HttpApi.Client.ConsoleTestApp;

[DependsOn(typeof(AbpAutofacModule))]
[DependsOn(typeof(PaymentHttpApiClientModule))]
[DependsOn(typeof(AbpHttpClientIdentityModelModule))]
public class PaymentConsoleApiClientModule : AbpModule { }
