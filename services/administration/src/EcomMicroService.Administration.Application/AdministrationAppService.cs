using EcomMicroService.Administration.Localization;
using Volo.Abp.Application.Services;

namespace EcomMicroService.Administration;

public abstract class AdministrationAppService : ApplicationService
{
    protected AdministrationAppService()
    {
        LocalizationResource = typeof(AdministrationResource);
        ObjectMapperContext = typeof(AdministrationApplicationModule);
    }
}
