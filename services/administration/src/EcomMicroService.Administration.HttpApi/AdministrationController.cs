using EcomMicroService.Administration.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace EcomMicroService.Administration;

public abstract class AdministrationController : AbpControllerBase
{
    protected AdministrationController()
    {
        LocalizationResource = typeof(AdministrationResource);
    }
}
