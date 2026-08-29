using System;
using System.Collections.Generic;

namespace EcomMicroService.Catalog;

/// <summary>
/// Brand option for storefront filter dropdown (id + name).
/// </summary>
public class BrandFilterItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Model option for storefront filter dropdown (id + brandId + name).
/// </summary>
public class ModelFilterItemDto
{
    public Guid Id { get; set; }
    public Guid BrandId { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Optional context to resolve storefront filter options.
/// </summary>
public class CatalogFilterOptionsRequestDto
{
    /// <summary>Optional category context; when set, facets are limited to products in this category.</summary>
    public Guid? CategoryId { get; set; }

    /// <summary>Optional product type context; when set, only this type's metadata is used.</summary>
    public Guid? ProductTypeId { get; set; }
}

/// <summary>
/// One allowed value for a metadata-driven filter facet with resolved labels.
/// Effective label: <see cref="DisplayName"/> ?? <see cref="FallbackDisplayName"/> ?? <see cref="Value"/>.
/// </summary>
public class CatalogAttributeFilterValueDto
{
    /// <summary>Invariant value stored on products (matches dynamic attribute JSON).</summary>
    public string Value { get; set; } = string.Empty;

    public string? DisplayName { get; set; }
    public string? DisplayNameLanguage { get; set; }
    public string? FallbackDisplayName { get; set; }
    public string? FallbackDisplayNameLanguage { get; set; }
}

/// <summary>
/// Metadata-driven attribute facet for storefront filters.
/// Effective facet title: <see cref="DisplayName"/> ?? <see cref="FallbackDisplayName"/> ?? <see cref="Key"/>.
/// </summary>
public class CatalogAttributeFilterItemDto
{
    public string Key { get; set; } = string.Empty;

    /// <summary>Resolved display name for the current UI culture when a translation exists.</summary>
    public string? DisplayName { get; set; }

    public string? DisplayNameLanguage { get; set; }
    public string? FallbackDisplayName { get; set; }
    public string? FallbackDisplayNameLanguage { get; set; }

    /// <summary>
    /// Localized entries for each distinct value present in the catalog context.
    /// </summary>
    public List<CatalogAttributeFilterValueDto> LocalizedValues { get; set; } = new();

    /// <summary>
    /// Raw invariant values only (backward compatibility). Prefer <see cref="LocalizedValues"/> for labels.
    /// </summary>
    public List<string> Values { get; set; } = new();
}

/// <summary>
/// Available filter values for storefront.
/// </summary>
public class CatalogFilterOptionsDto
{
    /// <summary>Metadata-driven attribute facets derived from product type definitions and product payloads.</summary>
    public List<CatalogAttributeFilterItemDto> Attributes { get; set; } = new();

    /// <summary>Brands that have at least one published product (for filter dropdown).</summary>
    public List<BrandFilterItemDto> Brands { get; set; } = new();

    /// <summary>Models that have at least one published product (for filter dropdown).</summary>
    public List<ModelFilterItemDto> Models { get; set; } = new();
}
