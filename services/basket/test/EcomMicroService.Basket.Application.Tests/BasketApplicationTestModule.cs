using Volo.Abp.Modularity;

namespace EcomMicroService.Basket;

[DependsOn(typeof(BasketApplicationModule))]
[DependsOn(typeof(BasketDomainTestModule))]
public class BasketApplicationTestModule : AbpModule { }
