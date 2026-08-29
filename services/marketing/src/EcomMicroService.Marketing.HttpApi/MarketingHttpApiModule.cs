using Localization.Resources.AbpUi;
using Microsoft.Extensions.DependencyInjection;
using EcomMicroService.Marketing.Localization;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;

namespace EcomMicroService.Marketing;

[DependsOn(typeof(MarketingApplicationContractsModule))]
[DependsOn(typeof(AbpAspNetCoreMvcModule))]
public class MarketingHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
            mvcBuilder.AddApplicationPartIfNotExists(typeof(MarketingHttpApiModule).Assembly);
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources.Get<MarketingResource>().AddBaseTypes(typeof(AbpUiResource));
        });
    }
}
