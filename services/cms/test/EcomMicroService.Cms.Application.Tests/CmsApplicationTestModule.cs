using Volo.Abp.Modularity;

namespace EcomMicroService.Cms;

[DependsOn(typeof(CmsApplicationModule))]
[DependsOn(typeof(CmsDomainTestModule))]
public class CmsApplicationTestModule : AbpModule { }
