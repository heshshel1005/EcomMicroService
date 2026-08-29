using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Volo.Abp.Application;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;

namespace EcomMicroService.Customer;

[DependsOn(typeof(CustomerDomainModule))]
[DependsOn(typeof(CustomerApplicationContractsModule))]
[DependsOn(typeof(AbpDddApplicationModule))]
[DependsOn(typeof(AbpMapperlyModule))]
public class CustomerApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<CustomerApplicationModule>();
    }
}
