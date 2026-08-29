using System;
using System.Collections.Generic;
using System.Linq;
using EcomMicroService.Catalog.Localization;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Catalog;

/// <summary>
/// Model belonging to a brand. Products may reference an optional model that must belong to the product's brand.
/// </summary>
public class BrandModel : AuditedAggregateRoot<Guid>, IMultiTenant, IMultiLingualEntity<BrandModelTranslation>
{
    public Guid? TenantId { get; set; }
    public Guid BrandId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsActive { get; set; }
    public virtual ICollection<BrandModelTranslation> Translations { get; set; } = new List<BrandModelTranslation>();

    public virtual Brand? Brand { get; set; }

    protected BrandModel()
    {
    }

    public BrandModel(
        Guid id,
        Guid brandId,
        string name,
        string? code = null,
        bool isActive = true)
        : base(id)
    {
        BrandId = brandId;
        Name = name ?? string.Empty;
        Code = code;
        IsActive = isActive;
    }

    public void SetTranslations(IEnumerable<BrandModelTranslation> translations, string defaultLanguage)
    {
        var translationList = translations?.ToList() ?? new List<BrandModelTranslation>();
        MultilingualDomainGuard.ValidateRequiredDefaultAndNoDuplicates(
            translationList,
            defaultLanguage,
            CatalogDomainErrorCodes.BrandModelDuplicateTranslationLanguage,
            CatalogDomainErrorCodes.BrandModelDefaultTranslationRequired);

        Translations.Clear();
        foreach (var translation in translationList)
        {
            translation.Language = MultilingualDomainGuard.NormalizeLanguage(translation.Language);
            translation.BrandModelId = Id;
            Translations.Add(translation);
        }
    }
}
