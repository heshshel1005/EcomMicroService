using EcomMicroService.Ordering.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace EcomMicroService.Ordering.Permissions;

public class OrderingPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.GetGroupOrNull(OrderingPermissions.GroupName)
                    ?? context.AddGroup(OrderingPermissions.GroupName, L("Permission:ECommerce"));

        if (group.GetPermissionOrNull(OrderingPermissions.Orders) == null)
        {
            group.AddPermission(OrderingPermissions.Orders, L("Permission:Orders"));
        }

        if (group.GetPermissionOrNull(OrderingPermissions.Analytics) == null)
        {
            group.AddPermission(OrderingPermissions.Analytics, L("Permission:Analytics"));
        }
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<OrderingResource>(name);
    }
}
