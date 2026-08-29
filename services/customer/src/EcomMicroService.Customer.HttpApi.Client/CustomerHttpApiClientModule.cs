using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace EcomMicroService.Customer;

[DependsOn(typeof(CustomerApplicationContractsModule))]
[DependsOn(typeof(AbpHttpClientModule))]
public class CustomerHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(CustomerApplicationContractsModule).Assembly,
            CustomerRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<CustomerHttpApiClientModule>();
        });
    }
}
