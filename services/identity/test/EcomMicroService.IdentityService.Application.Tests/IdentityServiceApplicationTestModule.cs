using Volo.Abp.Modularity;

namespace EcomMicroService.IdentityService;

[DependsOn(typeof(IdentityServiceApplicationModule))]
[DependsOn(typeof(IdentityServiceDomainTestModule))]
public class IdentityServiceApplicationTestModule : AbpModule { }
