using EcomMicroService.WebApp.Localization;
using Volo.Abp.AspNetCore.Components;

namespace EcomMicroService.WebApp.Blazor.Client;

public abstract class WebAppComponentBase : AbpComponentBase
{
    protected WebAppComponentBase()
    {
        LocalizationResource = typeof(WebAppResource);
    }
}
