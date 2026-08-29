using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace EcomMicroService.Basket;

[DependsOn(typeof(AbpDddDomainModule))]
[DependsOn(typeof(BasketDomainSharedModule))]
public class BasketDomainModule : AbpModule { }
