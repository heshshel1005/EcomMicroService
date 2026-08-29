using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;
using Volo.CmsKit;

namespace EcomMicroService.Cms;

[DependsOn(typeof(CmsDomainSharedModule))]
[DependsOn(typeof(AbpDddApplicationContractsModule))]
[DependsOn(typeof(AbpAuthorizationModule))]
[DependsOn(typeof(CmsKitApplicationContractsModule))]
public class CmsApplicationContractsModule : AbpModule { }
