using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;

namespace EcomMicroService.Payment;

[DependsOn(typeof(PaymentDomainSharedModule))]
[DependsOn(typeof(AbpDddApplicationContractsModule))]
[DependsOn(typeof(AbpAuthorizationModule))]
public class PaymentApplicationContractsModule : AbpModule { }
