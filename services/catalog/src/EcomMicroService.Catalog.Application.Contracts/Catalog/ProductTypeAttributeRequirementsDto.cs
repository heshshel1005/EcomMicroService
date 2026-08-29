using System;
using System.Collections.Generic;

namespace EcomMicroService.Catalog;

/// <summary>
/// Product-type-specific attribute definitions grouped by requirement level.
/// </summary>
public class ProductTypeAttributeRequirementsDto
{
    public Guid ProductTypeId { get; set; }
    public List<ProductTypeAttributeRequirementItemDto> RequiredAttributes { get; set; } = new();
    public List<ProductTypeAttributeRequirementItemDto> RecommendedAttributes { get; set; } = new();
    public List<ProductTypeAttributeRequirementItemDto> ConditionalAttributes { get; set; } = new();
}

/// <summary>
/// Attribute definition payload used by product-type requirement query.
/// </summary>
public class ProductTypeAttributeRequirementItemDto
{
    public Guid AttributeDefinitionId { get; set; }
    public string Key { get; set; } = string.Empty;
    public AttributeDefinitionDataType DataType { get; set; }
    /// <summary>
    /// Localized display name for the current UI culture when available.
    /// </summary>
    public string? DisplayName { get; set; }
    /// <summary>
    /// Language used for <see cref="DisplayName"/> (current UI culture if available, otherwise null).
    /// </summary>
    public string? DisplayNameLanguage { get; set; }
    /// <summary>
    /// Fallback display name resolved from tenant default language when current UI culture translation is missing.
    /// </summary>
    public string? FallbackDisplayName { get; set; }
    /// <summary>
    /// Language used for <see cref="FallbackDisplayName"/>.
    /// </summary>
    public string? FallbackDisplayNameLanguage { get; set; }
    /// <summary>
    /// Localized description for the current UI culture when available.
    /// </summary>
    public string? Description { get; set; }
    /// <summary>
    /// Language used for <see cref="Description"/> (current UI culture if available, otherwise null).
    /// </summary>
    public string? DescriptionLanguage { get; set; }
    /// <summary>
    /// Fallback description resolved from tenant default language when current UI culture translation is missing.
    /// </summary>
    public string? FallbackDescription { get; set; }
    /// <summary>
    /// Language used for <see cref="FallbackDescription"/>.
    /// </summary>
    public string? FallbackDescriptionLanguage { get; set; }
    public string? AllowedValuesJson { get; set; }
    /// <summary>
    /// Localized option labels for enum-like attributes.
    /// </summary>
    public List<ProductTypeAttributeOptionDto> LocalizedOptions { get; set; } = new();
    public string? RegexPattern { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsRequired { get; set; }
    public bool IsRecommended { get; set; }
    public string? ConditionalAttributeKey { get; set; }
    public ProductTypeRuleConditionOperator? ConditionalOperator { get; set; }
    public string? ConditionalExpectedValue { get; set; }
}

/// <summary>
/// Localized option metadata for enum-like dynamic attributes.
/// </summary>
public class ProductTypeAttributeOptionDto
{
    /// <summary>
    /// Invariant option value/code used in dynamic attribute payloads.
    /// </summary>
    public string Value { get; set; } = string.Empty;
    /// <summary>
    /// Localized option label for the current UI culture when available.
    /// </summary>
    public string? DisplayName { get; set; }
    /// <summary>
    /// Language used for <see cref="DisplayName"/> (current UI culture if available, otherwise null).
    /// </summary>
    public string? DisplayNameLanguage { get; set; }
    /// <summary>
    /// Fallback option label resolved from tenant default language when current UI culture translation is missing.
    /// </summary>
    public string? FallbackDisplayName { get; set; }
    /// <summary>
    /// Language used for <see cref="FallbackDisplayName"/>.
    /// </summary>
    public string? FallbackDisplayNameLanguage { get; set; }
}
