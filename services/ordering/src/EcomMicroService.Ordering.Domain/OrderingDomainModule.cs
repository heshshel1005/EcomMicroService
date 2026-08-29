using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace EcomMicroService.Ordering;

[DependsOn(typeof(AbpDddDomainModule))]
[DependsOn(typeof(OrderingDomainSharedModule))]
public class OrderingDomainModule : AbpModule { }
