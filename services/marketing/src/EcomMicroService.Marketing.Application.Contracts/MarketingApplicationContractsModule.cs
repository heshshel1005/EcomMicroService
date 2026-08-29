using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;

namespace EcomMicroService.Marketing;

[DependsOn(typeof(MarketingDomainSharedModule))]
[DependsOn(typeof(AbpDddApplicationContractsModule))]
[DependsOn(typeof(AbpAuthorizationModule))]
public class MarketingApplicationContractsModule : AbpModule { }
