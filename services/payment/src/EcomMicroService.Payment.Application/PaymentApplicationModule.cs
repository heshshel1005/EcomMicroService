using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Volo.Abp;
using Volo.Abp.Application;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;

namespace EcomMicroService.Payment;

[DependsOn(typeof(PaymentDomainModule))]
[DependsOn(typeof(PaymentApplicationContractsModule))]
[DependsOn(typeof(AbpDddApplicationModule))]
[DependsOn(typeof(AbpMapperlyModule))]
public class PaymentApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<PaymentApplicationModule>();
        context.Services.AddHttpClient("Ordering").ConfigurePrimaryHttpMessageHandler(() =>
            new System.Net.Http.HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });
        var configuration = context.Services.GetConfiguration();
        context.Services.Configure<PayPalPaymentGatewayOptions>(configuration.GetSection(PayPalPaymentGatewayOptions.SectionName));
        context.Services.Configure<PaymobPaymentGatewayOptions>(configuration.GetSection(PaymobPaymentGatewayOptions.SectionName));
        context.Services.AddTransient<IPaymentGateway, CashOnDeliveryPaymentGateway>();
        context.Services.AddTransient<IPaymentGateway, PayPalPaymentGateway>();
        context.Services.AddTransient<IPaymentGateway, PaymobPaymentGateway>();
    }
}
