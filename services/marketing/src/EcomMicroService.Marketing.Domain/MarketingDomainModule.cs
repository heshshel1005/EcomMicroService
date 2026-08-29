using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace EcomMicroService.Marketing;

[DependsOn(typeof(AbpDddDomainModule))]
[DependsOn(typeof(MarketingDomainSharedModule))]
public class MarketingDomainModule : AbpModule { }
