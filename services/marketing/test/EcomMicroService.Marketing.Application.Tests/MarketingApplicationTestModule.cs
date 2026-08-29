using Volo.Abp.Modularity;

namespace EcomMicroService.Marketing;

[DependsOn(typeof(MarketingApplicationModule))]
[DependsOn(typeof(MarketingDomainTestModule))]
public class MarketingApplicationTestModule : AbpModule { }
