using Volo.Abp.Reflection;

namespace EcomMicroService.Ordering.Permissions;

public class OrderingPermissions
{
    public const string GroupName = "ECommerce";
    public const string Administration = GroupName + ".Administration";
    public const string Orders = GroupName + ".Orders";
    public const string Analytics = GroupName + ".Analytics";

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(OrderingPermissions));
    }
}
