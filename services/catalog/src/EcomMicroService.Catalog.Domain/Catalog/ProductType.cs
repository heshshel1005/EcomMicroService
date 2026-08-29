using System;
using System.Collections.Generic;
using System.Linq;
using EcomMicroService.Catalog.Localization;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Catalog;

/// <summary>
/// Product type definition used to group products by business type (e.g. APPAREL, AUTO_PART).
/// Attribute definitions and validation rules are attached to product types.
/// </summary>
public class ProductType : AuditedAggregateRoot<Guid>, IMultiTenant, ICatalogTaxonomyEntity, IMultiLingualEntity<ProductTypeTranslation>
{
    public Guid? TenantId { get; set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public virtual ICollection<ProductTypeTranslation> Translations { get; set; } = new List<ProductTypeTranslation>();

    protected ProductType()
    {
    }

    public ProductType(Guid id, string code, string name, bool isActive = true)
        : base(id)
    {
        SetCode(code);
        SetName(name);
        IsActive = isActive;
    }

    public void SetCode(string code)
    {
        Code = NormalizeRequired(code, nameof(code));
    }

    public void SetName(string name)
    {
        Name = NormalizeRequired(name, nameof(name));
    }

    public void SetIsActive(bool isActive)
    {
        IsActive = isActive;
    }

    public void SetTranslations(IEnumerable<ProductTypeTranslation> translations, string defaultLanguage)
    {
        var translationList = translations?.ToList() ?? new List<ProductTypeTranslation>();
        MultilingualDomainGuard.ValidateRequiredDefaultAndNoDuplicates(
            translationList,
            defaultLanguage,
            CatalogDomainErrorCodes.ProductTypeDuplicateTranslationLanguage,
            CatalogDomainErrorCodes.ProductTypeDefaultTranslationRequired);

        Translations.Clear();
        foreach (var translation in translationList)
        {
            translation.Language = MultilingualDomainGuard.NormalizeLanguage(translation.Language);
            translation.ProductTypeId = Id;
            Translations.Add(translation);
        }
    }

    private static string NormalizeRequired(string value, string paramName)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (normalized.Length == 0)
        {
            throw new ArgumentException($"{paramName} is required.", paramName);
        }

        return normalized;
    }
}
