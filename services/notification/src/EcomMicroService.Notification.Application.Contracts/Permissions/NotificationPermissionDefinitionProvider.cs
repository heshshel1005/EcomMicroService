using EcomMicroService.Notification.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace EcomMicroService.Notification.Permissions;

public class NotificationPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        context.AddGroup(NotificationPermissions.GroupName, L("Permission:Notification"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<NotificationResource>(name);
    }
}
