using Localization.Resources.AbpUi;
using Microsoft.Extensions.DependencyInjection;
using EcomMicroService.Customer.Localization;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;

namespace EcomMicroService.Customer;

[DependsOn(typeof(CustomerApplicationContractsModule))]
[DependsOn(typeof(AbpAspNetCoreMvcModule))]
public class CustomerHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
            mvcBuilder.AddApplicationPartIfNotExists(typeof(CustomerHttpApiModule).Assembly);
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources.Get<CustomerResource>().AddBaseTypes(typeof(AbpUiResource));
        });
    }
}
