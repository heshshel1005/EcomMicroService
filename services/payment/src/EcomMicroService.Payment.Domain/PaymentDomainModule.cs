using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace EcomMicroService.Payment;

[DependsOn(typeof(AbpDddDomainModule))]
[DependsOn(typeof(PaymentDomainSharedModule))]
public class PaymentDomainModule : AbpModule { }
