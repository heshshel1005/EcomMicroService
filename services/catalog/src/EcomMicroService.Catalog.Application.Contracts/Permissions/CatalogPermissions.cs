using Volo.Abp.Reflection;

namespace EcomMicroService.Catalog.Permissions;

public class CatalogPermissions
{
    public const string GroupName = "ECommerce";

    public const string Administration = GroupName + ".Administration";

    public static class Catalog
    {
        public const string Default = GroupName + ".Catalog";
        public const string HostTaxonomy = Default + ".HostTaxonomy";
        public const string Brands = Default + ".Brands";
        public const string BrandModels = Default + ".BrandModels";
        public const string AttributeDefinitionsReview = Default + ".AttributeDefinitions.Review";
        public const string AttributeDefinitionsPublish = Default + ".AttributeDefinitions.Publish";
    }

    public static class Inventory
    {
        public const string Default = GroupName + ".Inventory";
    }

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(CatalogPermissions));
    }
}
