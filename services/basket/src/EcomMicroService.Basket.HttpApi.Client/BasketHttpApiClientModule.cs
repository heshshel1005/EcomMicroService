using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace EcomMicroService.Basket;

[DependsOn(typeof(BasketApplicationContractsModule))]
[DependsOn(typeof(AbpHttpClientModule))]
public class BasketHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(BasketApplicationContractsModule).Assembly,
            BasketRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<BasketHttpApiClientModule>();
        });
    }
}
