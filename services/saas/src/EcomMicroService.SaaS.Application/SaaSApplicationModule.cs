using Microsoft.Extensions.DependencyInjection;
using EcomMicroService.Administration;
using Volo.Abp.Application;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using Volo.Abp.TenantManagement;

namespace EcomMicroService.SaaS;

[DependsOn(typeof(AbpMapperlyModule))]
[DependsOn(typeof(AbpDddApplicationModule))]
[DependsOn(typeof(AbpTenantManagementApplicationModule))]
[DependsOn(typeof(AdministrationApplicationModule))]
[DependsOn(typeof(SaaSApplicationContractsModule))]
[DependsOn(typeof(SaaSDomainModule))]
public class SaaSApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<SaaSApplicationModule>();
    }
}
