using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;

namespace EcomMicroService.Ordering;

[DependsOn(typeof(OrderingDomainSharedModule))]
[DependsOn(typeof(AbpDddApplicationContractsModule))]
[DependsOn(typeof(AbpAuthorizationModule))]
public class OrderingApplicationContractsModule : AbpModule { }
