using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;

namespace EcomMicroService.Customer;

[DependsOn(typeof(CustomerDomainSharedModule))]
[DependsOn(typeof(AbpDddApplicationContractsModule))]
[DependsOn(typeof(AbpAuthorizationModule))]
public class CustomerApplicationContractsModule : AbpModule { }
