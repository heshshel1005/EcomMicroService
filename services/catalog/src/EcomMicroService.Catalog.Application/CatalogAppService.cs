using EcomMicroService.Catalog.Localization;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization.Permissions;

namespace EcomMicroService.Catalog;

public abstract class CatalogAppService : ApplicationService
{
    protected IPermissionChecker PermissionChecker => LazyServiceProvider.LazyGetRequiredService<IPermissionChecker>();

    protected CatalogAppService()
    {
        LocalizationResource = typeof(CatalogResource);
        ObjectMapperContext = typeof(CatalogApplicationModule);
    }
}
