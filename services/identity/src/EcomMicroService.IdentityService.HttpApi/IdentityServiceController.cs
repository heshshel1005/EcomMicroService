using EcomMicroService.IdentityService.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace EcomMicroService.IdentityService;

public abstract class IdentityServiceController : AbpControllerBase
{
    protected IdentityServiceController()
    {
        LocalizationResource = typeof(IdentityServiceResource);
    }
}
