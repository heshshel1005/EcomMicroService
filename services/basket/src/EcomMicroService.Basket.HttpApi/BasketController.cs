using EcomMicroService.Basket.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace EcomMicroService.Basket;

public abstract class BasketController : AbpControllerBase
{
    protected BasketController()
    {
        LocalizationResource = typeof(BasketResource);
    }
}
