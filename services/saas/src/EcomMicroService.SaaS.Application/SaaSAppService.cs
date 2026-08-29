using EcomMicroService.SaaS.Localization;
using Volo.Abp.Application.Services;

namespace EcomMicroService.SaaS;

public abstract class SaaSAppService : ApplicationService
{
    protected SaaSAppService()
    {
        LocalizationResource = typeof(SaaSResource);
        ObjectMapperContext = typeof(SaaSApplicationModule);
    }
}
