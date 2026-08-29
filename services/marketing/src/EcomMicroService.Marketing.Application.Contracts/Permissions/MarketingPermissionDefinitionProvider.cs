using EcomMicroService.Marketing.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace EcomMicroService.Marketing.Permissions;

public class MarketingPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        context.AddGroup(MarketingPermissions.GroupName, L("Permission:Marketing"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MarketingResource>(name);
    }
}
