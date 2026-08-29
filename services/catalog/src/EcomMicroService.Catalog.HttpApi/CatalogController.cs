using EcomMicroService.Catalog.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace EcomMicroService.Catalog;

public abstract class CatalogController : AbpControllerBase
{
    protected CatalogController()
    {
        LocalizationResource = typeof(CatalogResource);
    }
}
