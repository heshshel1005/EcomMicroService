using EcomMicroService.Basket.Localization;
using Volo.Abp.Application.Services;

namespace EcomMicroService.Basket;

public abstract class BasketAppService : ApplicationService
{
    protected BasketAppService()
    {
        LocalizationResource = typeof(BasketResource);
        ObjectMapperContext = typeof(BasketApplicationModule);
    }
}
