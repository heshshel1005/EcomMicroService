using Localization.Resources.AbpUi;
using Microsoft.Extensions.DependencyInjection;
using EcomMicroService.Notification.Localization;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;

namespace EcomMicroService.Notification;

[DependsOn(typeof(NotificationApplicationContractsModule))]
[DependsOn(typeof(AbpAspNetCoreMvcModule))]
public class NotificationHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
            mvcBuilder.AddApplicationPartIfNotExists(typeof(NotificationHttpApiModule).Assembly);
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources.Get<NotificationResource>().AddBaseTypes(typeof(AbpUiResource));
        });
    }
}
