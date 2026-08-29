using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Catalog;

/// <summary>
/// Localized text values for <see cref="AttributeDefinition"/>.
/// </summary>
public class AttributeDefinitionTranslation : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid AttributeDefinitionId { get; private set; }
    public string Language { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public virtual AttributeDefinition? AttributeDefinition { get; set; }

    protected AttributeDefinitionTranslation()
    {
    }

    public AttributeDefinitionTranslation(
        Guid id,
        Guid attributeDefinitionId,
        string language,
        string displayName,
        string? description = null)
        : base(id)
    {
        SetAttributeDefinition(attributeDefinitionId);
        SetLanguage(language);
        SetDisplayName(displayName);
        SetDescription(description);
    }

    public void SetAttributeDefinition(Guid attributeDefinitionId)
    {
        if (attributeDefinitionId == Guid.Empty)
        {
            throw new ArgumentException("attributeDefinitionId is required.", nameof(attributeDefinitionId));
        }

        AttributeDefinitionId = attributeDefinitionId;
    }

    public void SetLanguage(string language)
    {
        Language = NormalizeRequired(language, CatalogConsts.Catalog.TranslationLanguageMaxLength, nameof(language));
    }

    public void SetDisplayName(string displayName)
    {
        DisplayName = NormalizeRequired(displayName, CatalogConsts.Catalog.AttributeDefinitionTranslationDisplayNameMaxLength, nameof(displayName));
    }

    public void SetDescription(string? description)
    {
        Description = NormalizeOptional(description, CatalogConsts.Catalog.AttributeDefinitionTranslationDescriptionMaxLength, nameof(description));
    }

    private static string NormalizeRequired(string value, int maxLength, string paramName)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (normalized.Length == 0)
        {
            throw new ArgumentException($"{paramName} is required.", paramName);
        }

        if (normalized.Length > maxLength)
        {
            throw new ArgumentException($"{paramName} exceeds max length of {maxLength}.", paramName);
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value, int maxLength, string paramName)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        if (normalized.Length > maxLength)
        {
            throw new ArgumentException($"{paramName} exceeds max length of {maxLength}.", paramName);
        }

        return normalized;
    }
}
