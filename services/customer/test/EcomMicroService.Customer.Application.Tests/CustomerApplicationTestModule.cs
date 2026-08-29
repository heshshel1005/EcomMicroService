using Volo.Abp.Modularity;

namespace EcomMicroService.Customer;

[DependsOn(typeof(CustomerApplicationModule))]
[DependsOn(typeof(CustomerDomainTestModule))]
public class CustomerApplicationTestModule : AbpModule { }
