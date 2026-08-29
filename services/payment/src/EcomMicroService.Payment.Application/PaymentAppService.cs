using EcomMicroService.Payment.Localization;
using Volo.Abp.Application.Services;

namespace EcomMicroService.Payment;

public abstract class PaymentAppService : ApplicationService
{
    protected PaymentAppService()
    {
        LocalizationResource = typeof(PaymentResource);
        ObjectMapperContext = typeof(PaymentApplicationModule);
    }
}
