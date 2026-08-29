using System;
using System.Collections.Generic;
using System.Linq;
using EcomMicroService.Catalog.Localization;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Catalog;

/// <summary>
/// Brand aggregate root. Has many models; products reference a brand (and optionally a model).
/// </summary>
public class Brand : AuditedAggregateRoot<Guid>, IMultiTenant, IMultiLingualEntity<BrandTranslation>
{
    public Guid? TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public virtual ICollection<BrandTranslation> Translations { get; set; } = new List<BrandTranslation>();

    /// <summary>Navigation: models belonging to this brand.</summary>
    public virtual ICollection<BrandModel> Models { get; set; } = new List<BrandModel>();

    protected Brand()
    {
    }

    public Brand(
        Guid id,
        string name,
        string? slug = null,
        string? description = null,
        bool isActive = true)
        : base(id)
    {
        Name = name ?? string.Empty;
        Slug = slug;
        Description = description;
        IsActive = isActive;
    }

    public void SetTranslations(IEnumerable<BrandTranslation> translations, string defaultLanguage)
    {
        var translationList = translations?.ToList() ?? new List<BrandTranslation>();
        MultilingualDomainGuard.ValidateRequiredDefaultAndNoDuplicates(
            translationList,
            defaultLanguage,
            CatalogDomainErrorCodes.BrandDuplicateTranslationLanguage,
            CatalogDomainErrorCodes.BrandDefaultTranslationRequired);

        Translations.Clear();
        foreach (var translation in translationList)
        {
            translation.Language = MultilingualDomainGuard.NormalizeLanguage(translation.Language);
            translation.BrandId = Id;
            Translations.Add(translation);
        }
    }
}
