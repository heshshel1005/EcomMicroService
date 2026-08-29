using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using EcomMicroService.Catalog.Localization;

namespace EcomMicroService.Catalog;

/// <summary>
/// Category in a tree structure for organizing products (parent/child, slug, display order).
/// Implements <see cref="ICatalogTaxonomyEntity"/>: host-shared categories use <see cref="TenantId"/> null; tenants may add their own nodes and reference host parents.
/// </summary>
public class Category : AuditedAggregateRoot<Guid>, IMultiTenant, ICatalogTaxonomyEntity, IMultiLingualEntity<CategoryTranslation>
{
    public Guid? TenantId { get; set; }
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public virtual ICollection<CategoryTranslation> Translations { get; set; } = new List<CategoryTranslation>();

    protected Category()
    {
    }

    public Category(Guid id, string name, string slug, Guid? parentId = null, int displayOrder = 0)
        : base(id)
    {
        Name = name ?? string.Empty;
        Slug = slug ?? string.Empty;
        ParentId = parentId;
        DisplayOrder = displayOrder;
    }

    public void SetTranslations(IEnumerable<CategoryTranslation> translations, string defaultLanguage)
    {
        var translationList = translations?.ToList() ?? new List<CategoryTranslation>();
        MultilingualDomainGuard.ValidateRequiredDefaultAndNoDuplicates(
            translationList,
            defaultLanguage,
            CatalogDomainErrorCodes.CategoryDuplicateTranslationLanguage,
            CatalogDomainErrorCodes.CategoryDefaultTranslationRequired);

        Translations.Clear();
        foreach (var translation in translationList)
        {
            translation.Language = MultilingualDomainGuard.NormalizeLanguage(translation.Language);
            translation.CategoryId = Id;
            Translations.Add(translation);
        }
    }
}
