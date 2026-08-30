using System.Linq;
using EcomMicroService.Catalog.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Catalog.Permissions;

public class CatalogPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.GetGroupOrNull(CatalogPermissions.GroupName)
                    ?? context.AddGroup(CatalogPermissions.GroupName, L("Permission:ECommerce"));

        var administration = group.GetPermissionOrNull(CatalogPermissions.Administration)
                             ?? group.AddPermission(CatalogPermissions.Administration, L("Permission:Administration"));
        administration.MultiTenancySide = MultiTenancySides.Both;

        var catalog = GetOrAddChild(administration, CatalogPermissions.Catalog.Default, L("Permission:Catalog"));
        catalog.MultiTenancySide = MultiTenancySides.Both;

        var hostCatalogTaxonomy = GetOrAddChild(administration, CatalogPermissions.Catalog.HostTaxonomy, L("Permission:Catalog.HostTaxonomy"));
        hostCatalogTaxonomy.MultiTenancySide = MultiTenancySides.Host;

        var brands = GetOrAddChild(catalog, CatalogPermissions.Catalog.Brands, L("Permission:Catalog.Brands"));
        brands.MultiTenancySide = MultiTenancySides.Both;

        var brandModels = GetOrAddChild(catalog, CatalogPermissions.Catalog.BrandModels, L("Permission:Catalog.BrandModels"));
        brandModels.MultiTenancySide = MultiTenancySides.Both;

        var attributeDefinitionsReview = GetOrAddChild(catalog, CatalogPermissions.Catalog.AttributeDefinitionsReview, L("Permission:Catalog.AttributeDefinitions.Review"));
        attributeDefinitionsReview.MultiTenancySide = MultiTenancySides.Both;

        var attributeDefinitionsPublish = GetOrAddChild(catalog, CatalogPermissions.Catalog.AttributeDefinitionsPublish, L("Permission:Catalog.AttributeDefinitions.Publish"));
        attributeDefinitionsPublish.MultiTenancySide = MultiTenancySides.Both;

        GetOrAddChild(administration, CatalogPermissions.Inventory.Default, L("Permission:Inventory"));
    }

    private static PermissionDefinition GetOrAddChild(
        PermissionDefinition parent,
        string name,
        ILocalizableString displayName)
    {
        return parent.Children.FirstOrDefault(x => x.Name == name)
               ?? parent.AddChild(name, displayName);
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<CatalogResource>(name);
    }
}
