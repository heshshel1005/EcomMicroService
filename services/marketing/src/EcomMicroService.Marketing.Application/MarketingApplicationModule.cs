using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Volo.Abp.Application;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;

namespace EcomMicroService.Marketing;

[DependsOn(typeof(MarketingDomainModule))]
[DependsOn(typeof(MarketingApplicationContractsModule))]
[DependsOn(typeof(AbpDddApplicationModule))]
[DependsOn(typeof(AbpMapperlyModule))]
public class MarketingApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<MarketingApplicationModule>();
        context.Services.AddHttpClient("Catalog").ConfigurePrimaryHttpMessageHandler(() =>
            new System.Net.Http.HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });
    }
}
