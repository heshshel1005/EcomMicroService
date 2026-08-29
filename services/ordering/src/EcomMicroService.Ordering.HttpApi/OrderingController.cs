using EcomMicroService.Ordering.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace EcomMicroService.Ordering;

public abstract class OrderingController : AbpControllerBase
{
    protected OrderingController()
    {
        LocalizationResource = typeof(OrderingResource);
    }
}
