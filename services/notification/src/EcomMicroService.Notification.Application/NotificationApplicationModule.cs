using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Volo.Abp.Application;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;

namespace EcomMicroService.Notification;

[DependsOn(typeof(NotificationDomainModule))]
[DependsOn(typeof(NotificationApplicationContractsModule))]
[DependsOn(typeof(AbpDddApplicationModule))]
[DependsOn(typeof(AbpMapperlyModule))]
public class NotificationApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<NotificationApplicationModule>();
        context.Services.AddHttpClient("Catalog").ConfigurePrimaryHttpMessageHandler(() =>
            new System.Net.Http.HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });
    }
}
