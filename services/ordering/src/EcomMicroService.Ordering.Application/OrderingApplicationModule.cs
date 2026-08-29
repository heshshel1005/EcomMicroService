using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Application;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;

namespace EcomMicroService.Ordering;

[DependsOn(typeof(OrderingDomainModule))]
[DependsOn(typeof(OrderingApplicationContractsModule))]
[DependsOn(typeof(AbpDddApplicationModule))]
[DependsOn(typeof(AbpMapperlyModule))]
public class OrderingApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<OrderingApplicationModule>();
        context.Services.AddHttpClient("Basket").ConfigurePrimaryHttpMessageHandler(() =>
            new System.Net.Http.HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = System.Net.Http.HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });
        context.Services.AddHttpClient("Catalog").ConfigurePrimaryHttpMessageHandler(() =>
            new System.Net.Http.HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = System.Net.Http.HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });
        context.Services.AddHttpClient("Marketing").ConfigurePrimaryHttpMessageHandler(() =>
            new System.Net.Http.HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = System.Net.Http.HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });
        context.Services.AddHttpClient("Payment").ConfigurePrimaryHttpMessageHandler(() =>
            new System.Net.Http.HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = System.Net.Http.HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });
    }
}
