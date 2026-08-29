using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Application;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;

namespace EcomMicroService.Administration;

[DependsOn(typeof(AbpMapperlyModule))]
[DependsOn(typeof(AbpDddApplicationModule))]
[DependsOn(typeof(AbpFeatureManagementApplicationModule))]
[DependsOn(typeof(AbpPermissionManagementApplicationModule))]
[DependsOn(typeof(AbpSettingManagementApplicationModule))]
[DependsOn(typeof(AdministrationApplicationContractsModule))]
[DependsOn(typeof(AdministrationDomainModule))]
public class AdministrationApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<AdministrationApplicationModule>();
    }
}
