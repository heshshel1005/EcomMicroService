using System;
using System.Collections.Generic;

namespace EcomMicroService.Catalog;

public class AttributeDefinitionDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public AttributeDefinitionDataType DataType { get; set; }
    public string? AllowedValuesJson { get; set; }
    public string? RegexPattern { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public bool IsRequired { get; set; }
    public bool IsRecommended { get; set; }

    public AttributeDefinitionGovernanceStatus GovernanceStatus { get; set; }
    public int PublishedVersion { get; set; }

    /// <summary>
    /// Resolved display name for the current or requested culture.
    /// </summary>
    public string? DisplayName { get; set; }

    public string? DisplayNameLanguage { get; set; }

    /// <summary>
    /// Display name from fallback resolution when no translation matches the primary culture.
    /// </summary>
    public string? FallbackDisplayName { get; set; }

    public string? FallbackDisplayNameLanguage { get; set; }

    /// <summary>
    /// Resolved description for the same culture as <see cref="DisplayName"/>.
    /// </summary>
    public string? Description { get; set; }

    public List<AttributeDefinitionTranslationDto> Translations { get; set; } = new();
}
