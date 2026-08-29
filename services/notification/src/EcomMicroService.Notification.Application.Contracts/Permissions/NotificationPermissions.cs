using Volo.Abp.Reflection;

namespace EcomMicroService.Notification.Permissions;

public class NotificationPermissions
{
    public const string GroupName = "Notification";

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(NotificationPermissions));
    }
}
