using Volo.Abp.Reflection;

namespace EcomMicroService.Cms.Permissions;

public class CmsPermissions
{
    public const string GroupName = "Cms";

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(CmsPermissions));
    }
}
