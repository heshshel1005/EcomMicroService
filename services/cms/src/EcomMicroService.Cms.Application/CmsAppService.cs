using EcomMicroService.Cms.Localization;
using Volo.Abp.Application.Services;

namespace EcomMicroService.Cms;

public abstract class CmsAppService : ApplicationService
{
    protected CmsAppService()
    {
        LocalizationResource = typeof(CmsResource);
        ObjectMapperContext = typeof(CmsApplicationModule);
    }
}
