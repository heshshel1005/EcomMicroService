using Volo.Abp.Reflection;

namespace EcomMicroService.Marketing.Permissions;

public class MarketingPermissions
{
    public const string GroupName = "Marketing";

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(MarketingPermissions));
    }
}
