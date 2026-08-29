using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace EcomMicroService;

[Dependency(ReplaceServices = true)]
public class EcomMicroServiceBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "EcomMicroService";
}
