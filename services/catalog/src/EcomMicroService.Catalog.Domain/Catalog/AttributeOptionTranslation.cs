using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Catalog;

/// <summary>
/// Localized label values for a tenant-scoped attribute option.
/// </summary>
public class AttributeOptionTranslation : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid AttributeOptionId { get; private set; }
    public string Language { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;

    public virtual AttributeOption? AttributeOption { get; set; }

    protected AttributeOptionTranslation()
    {
    }

    public AttributeOptionTranslation(
        Guid id,
        Guid attributeOptionId,
        string language,
        string displayName)
        : base(id)
    {
        SetAttributeOption(attributeOptionId);
        SetLanguage(language);
        SetDisplayName(displayName);
    }

    public void SetAttributeOption(Guid attributeOptionId)
    {
        if (attributeOptionId == Guid.Empty)
        {
            throw new ArgumentException("attributeOptionId is required.", nameof(attributeOptionId));
        }

        AttributeOptionId = attributeOptionId;
    }

    public void SetLanguage(string language)
    {
        Language = NormalizeRequired(language, CatalogConsts.Catalog.TranslationLanguageMaxLength, nameof(language));
    }

    public void SetDisplayName(string displayName)
    {
        DisplayName = NormalizeRequired(displayName, CatalogConsts.Catalog.AttributeOptionTranslationDisplayNameMaxLength, nameof(displayName));
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
}
