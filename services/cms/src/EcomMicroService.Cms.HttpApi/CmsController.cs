using EcomMicroService.Cms.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace EcomMicroService.Cms;

public abstract class CmsController : AbpControllerBase
{
    protected CmsController()
    {
        LocalizationResource = typeof(CmsResource);
    }
}
