using System;
using System.Collections.Generic;
using System.Linq;
using EcomMicroService.Catalog.Localization;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Catalog;

/// <summary>
/// Product aggregate root. Has a unique product number; contains variants, media, category reference, and brand/model.
/// </summary>
public class Product : AuditedAggregateRoot<Guid>, IMultiTenant, IMultiLingualEntity<ProductTranslation>
{
    public Guid? TenantId { get; set; }
    public string ProductNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    /// <summary>Required. Product must belong to a brand.</summary>
    public Guid BrandId { get; set; }
    /// <summary>Optional. When set, must reference a model whose BrandId equals this product's BrandId.</summary>
    public Guid? ModelId { get; set; }
    /// <summary>Optional tenant-scoped product type assignment for dynamic attributes.</summary>
    public Guid? ProductTypeId { get; set; }
    /// <summary>
    /// JSON payload for type-specific dynamic attributes. Uses invariant keys/codes and is optional for backward compatibility.
    /// </summary>
    public string? DynamicAttributesJson { get; set; }
    public bool IsPublished { get; set; }
    public virtual ICollection<ProductTranslation> Translations { get; set; } = new List<ProductTranslation>();

    // Navigation properties for EF Core
    public virtual Brand? Brand { get; set; }
    public virtual BrandModel? Model { get; set; }

    protected Product()
    {
    }

    public Product(
        Guid id,
        string productNumber,
        string name,
        Guid brandId,
        string? description = null,
        Guid? categoryId = null,
        Guid? modelId = null,
        bool isPublished = false,
        Guid? productTypeId = null,
        string? dynamicAttributesJson = null)
        : base(id)
    {
        ProductNumber = productNumber ?? string.Empty;
        Name = name ?? string.Empty;
        BrandId = brandId;
        Description = description;
        CategoryId = categoryId;
        ModelId = modelId;
        ProductTypeId = productTypeId;
        DynamicAttributesJson = dynamicAttributesJson;
        IsPublished = isPublished;
    }

    /// <summary>
    /// Sets brand and optional model, enforcing the invariant that when model is set it must belong to the given brand.
    /// Call this from the application layer after loading the model's BrandId (pass it as modelBrandId).
    /// </summary>
    /// <param name="brandId">The brand to assign.</param>
    /// <param name="modelId">Optional model; when not null, must belong to brandId.</param>
    /// <param name="modelBrandId">The BrandId of the model when modelId is set; used to validate the invariant.</param>
    public void SetBrandAndModel(Guid brandId, Guid? modelId, Guid? modelBrandId)
    {
        if (modelId.HasValue && (!modelBrandId.HasValue || modelBrandId.Value != brandId))
        {
            throw new InvalidOperationException(
                "When a product has a model, the model must belong to the product's brand. " +
                "ModelId was set but the model's BrandId does not match the given brandId.");
        }

        BrandId = brandId;
        ModelId = modelId;
    }

    public void SetTranslations(IEnumerable<ProductTranslation> translations, string defaultLanguage)
    {
        var translationList = translations?.ToList() ?? new List<ProductTranslation>();
        MultilingualDomainGuard.ValidateRequiredDefaultAndNoDuplicates(
            translationList,
            defaultLanguage,
            CatalogDomainErrorCodes.ProductDuplicateTranslationLanguage,
            CatalogDomainErrorCodes.ProductDefaultTranslationRequired);

        Translations.Clear();
        foreach (var translation in translationList)
        {
            translation.Language = MultilingualDomainGuard.NormalizeLanguage(translation.Language);
            translation.ProductId = Id;
            Translations.Add(translation);
        }
    }
}
