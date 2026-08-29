using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace EcomMicroService.Cms;

[DependsOn(typeof(CmsApplicationContractsModule))]
[DependsOn(typeof(AbpHttpClientModule))]
public class CmsHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(CmsApplicationContractsModule).Assembly,
            CmsRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<CmsHttpApiClientModule>();
        });
    }
}
