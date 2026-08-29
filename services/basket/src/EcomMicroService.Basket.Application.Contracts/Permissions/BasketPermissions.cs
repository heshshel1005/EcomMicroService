using Volo.Abp.Reflection;

namespace EcomMicroService.Basket.Permissions;

public class BasketPermissions
{
    public const string GroupName = "Basket";

    public static class Issues
    {
        public const string Default = GroupName + ".Issues";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(BasketPermissions));
    }
}
