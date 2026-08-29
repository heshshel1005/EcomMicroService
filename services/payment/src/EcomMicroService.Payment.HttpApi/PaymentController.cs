using EcomMicroService.Payment.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace EcomMicroService.Payment;

public abstract class PaymentController : AbpControllerBase
{
    protected PaymentController()
    {
        LocalizationResource = typeof(PaymentResource);
    }
}
