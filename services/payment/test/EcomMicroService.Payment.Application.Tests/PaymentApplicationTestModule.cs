using Volo.Abp.Modularity;

namespace EcomMicroService.Payment;

[DependsOn(typeof(PaymentApplicationModule))]
[DependsOn(typeof(PaymentDomainTestModule))]
public class PaymentApplicationTestModule : AbpModule { }
