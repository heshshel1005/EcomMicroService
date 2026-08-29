using EcomMicroService.Marketing.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace EcomMicroService.Marketing;

public abstract class MarketingController : AbpControllerBase
{
    protected MarketingController()
    {
        LocalizationResource = typeof(MarketingResource);
    }
}
