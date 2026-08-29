using EcomMicroService.Basket.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace EcomMicroService.Basket.Permissions;

public class BasketPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var basketGroup = context.AddGroup(
            BasketPermissions.GroupName,
            L("Permission:Basket")
        );
        var basketPermissions = basketGroup.AddPermission(
            BasketPermissions.Issues.Default,
            L("Permission:Basket:Issues")
        );
        basketPermissions.AddChild(
            BasketPermissions.Issues.Create,
            L("Permission:Basket:Issues:Create")
        );
        basketPermissions.AddChild(
            BasketPermissions.Issues.Update,
            L("Permission:Basket:Issues:Update")
        );
        basketPermissions.AddChild(
            BasketPermissions.Issues.Delete,
            L("Permission:Basket:Issues:Delete")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<BasketResource>(name);
    }
}
