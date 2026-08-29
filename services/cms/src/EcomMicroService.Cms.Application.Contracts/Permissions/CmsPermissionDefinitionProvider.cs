using EcomMicroService.Cms.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace EcomMicroService.Cms.Permissions;

public class CmsPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        context.AddGroup(CmsPermissions.GroupName, L("Permission:Cms"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<CmsResource>(name);
    }
}
