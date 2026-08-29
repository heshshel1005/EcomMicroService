using EcomMicroService.Marketing.Localization;
using Volo.Abp.Application.Services;

namespace EcomMicroService.Marketing;

public abstract class MarketingAppService : ApplicationService
{
    protected MarketingAppService()
    {
        LocalizationResource = typeof(MarketingResource);
        ObjectMapperContext = typeof(MarketingApplicationModule);
    }
}
