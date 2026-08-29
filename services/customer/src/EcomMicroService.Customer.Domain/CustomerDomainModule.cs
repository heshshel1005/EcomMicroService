using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace EcomMicroService.Customer;

[DependsOn(typeof(AbpDddDomainModule))]
[DependsOn(typeof(CustomerDomainSharedModule))]
public class CustomerDomainModule : AbpModule { }
