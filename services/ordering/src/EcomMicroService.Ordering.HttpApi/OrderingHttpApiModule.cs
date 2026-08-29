using Localization.Resources.AbpUi;
using Microsoft.Extensions.DependencyInjection;
using EcomMicroService.Ordering.Localization;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;

namespace EcomMicroService.Ordering;

[DependsOn(typeof(OrderingApplicationContractsModule))]
[DependsOn(typeof(AbpAspNetCoreMvcModule))]
public class OrderingHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
            mvcBuilder.AddApplicationPartIfNotExists(typeof(OrderingHttpApiModule).Assembly);
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources.Get<OrderingResource>().AddBaseTypes(typeof(AbpUiResource));
        });
    }
}
