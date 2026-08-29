namespace EcomMicroService.Catalog;

public static class CatalogDomainErrorCodes
{
    public const string TranslationLanguageRequired = "ECommerce:TranslationLanguageRequired";

    public const string CategoryDuplicateTranslationLanguage = "ECommerce:CategoryDuplicateTranslationLanguage";
    public const string CategoryDefaultTranslationRequired = "ECommerce:CategoryDefaultTranslationRequired";

    public const string BrandDuplicateTranslationLanguage = "ECommerce:BrandDuplicateTranslationLanguage";
    public const string BrandDefaultTranslationRequired = "ECommerce:BrandDefaultTranslationRequired";

    public const string BrandModelDuplicateTranslationLanguage = "ECommerce:BrandModelDuplicateTranslationLanguage";
    public const string BrandModelDefaultTranslationRequired = "ECommerce:BrandModelDefaultTranslationRequired";

    public const string ProductDuplicateTranslationLanguage = "ECommerce:ProductDuplicateTranslationLanguage";
    public const string ProductDefaultTranslationRequired = "ECommerce:ProductDefaultTranslationRequired";

    public const string ProductTypeDuplicateTranslationLanguage = "ECommerce:ProductTypeDuplicateTranslationLanguage";
    public const string ProductTypeDefaultTranslationRequired = "ECommerce:ProductTypeDefaultTranslationRequired";

    public const string AttributeDefinitionDuplicateTranslationLanguage = "ECommerce:AttributeDefinitionDuplicateTranslationLanguage";
    public const string AttributeDefinitionDefaultTranslationRequired = "ECommerce:AttributeDefinitionDefaultTranslationRequired";

    public const string AttributeOptionTranslationUnknownValue = "ECommerce:AttributeOptionTranslationUnknownValue";
    public const string AttributeOptionTranslationDuplicateLanguage = "ECommerce:AttributeOptionTranslationDuplicateLanguage";
    public const string AttributeOptionTranslationDefaultRequired = "ECommerce:AttributeOptionTranslationDefaultRequired";

    public const string AttributeDefinitionInvalidGovernanceTransition = "ECommerce:AttributeDefinitionInvalidGovernanceTransition";
    public const string AttributeDefinitionArchivedMutationBlocked = "ECommerce:AttributeDefinitionArchivedMutationBlocked";
    public const string AttributeDefinitionMustBePublishedForProductTypeRules = "ECommerce:AttributeDefinitionMustBePublishedForProductTypeRules";
}
