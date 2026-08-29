namespace EcomMicroService.Catalog;

internal static class AttributeDefinitionCatalogGovernance
{
    public static bool IsPublishedForCatalog(AttributeDefinition definition) =>
        definition.GovernanceStatus == AttributeDefinitionGovernanceStatus.Published;
}
