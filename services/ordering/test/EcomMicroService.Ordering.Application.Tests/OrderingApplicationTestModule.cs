using Volo.Abp.Modularity;

namespace EcomMicroService.Ordering;

[DependsOn(typeof(OrderingApplicationModule))]
[DependsOn(typeof(OrderingDomainTestModule))]
public class OrderingApplicationTestModule : AbpModule { }
