using EcomMicroService.SaaS.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace EcomMicroService.SaaS;

public abstract class SaaSController : AbpControllerBase
{
    protected SaaSController()
    {
        LocalizationResource = typeof(SaaSResource);
    }
}
