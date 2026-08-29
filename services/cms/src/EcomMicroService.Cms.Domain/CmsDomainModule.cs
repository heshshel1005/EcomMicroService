using Volo.Abp.Domain;
using Volo.Abp.Modularity;
using Volo.CmsKit;

namespace EcomMicroService.Cms;

[DependsOn(typeof(AbpDddDomainModule))]
[DependsOn(typeof(CmsDomainSharedModule))]
[DependsOn(typeof(CmsKitDomainModule))]
public class CmsDomainModule : AbpModule { }
