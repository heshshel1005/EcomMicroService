using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Volo.CmsKit;
using Volo.Abp.Application;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;

namespace EcomMicroService.Cms;

[DependsOn(typeof(CmsDomainModule))]
[DependsOn(typeof(CmsApplicationContractsModule))]
[DependsOn(typeof(AbpDddApplicationModule))]
[DependsOn(typeof(AbpMapperlyModule))]
[DependsOn(typeof(CmsKitApplicationModule))]
public class CmsApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<CmsApplicationModule>();
        context.Services.AddHttpClient("Catalog").ConfigurePrimaryHttpMessageHandler(() =>
            new System.Net.Http.HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });
    }
}
