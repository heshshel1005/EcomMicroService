using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcomMicroService.Catalog;

/// <summary>
/// Localized metadata for one invariant enum/list value (see <see cref="AttributeDefinition.AllowedValuesJson"/>).
/// </summary>
public class AttributeOptionTranslationDto
{
    public string Value { get; set; } = string.Empty;

    public Guid OptionId { get; set; }

    public string? DisplayName { get; set; }

    public string? DisplayNameLanguage { get; set; }

    public string? FallbackDisplayName { get; set; }

    public string? FallbackDisplayNameLanguage { get; set; }

    public List<AttributeOptionTranslationItemDto> Translations { get; set; } = new();
}

public class AttributeOptionTranslationItemDto
{
    [Required]
    [StringLength(CatalogConsts.Catalog.TranslationLanguageMaxLength)]
    public string Language { get; set; } = string.Empty;

    [Required]
    [StringLength(CatalogConsts.Catalog.AttributeOptionTranslationDisplayNameMaxLength)]
    public string DisplayName { get; set; } = string.Empty;
}

public class SaveAttributeOptionTranslationsDto
{
    [Required]
    public List<AttributeOptionTranslationsInputDto> Options { get; set; } = new();
}

public class AttributeOptionTranslationsInputDto
{
    [Required]
    public string Value { get; set; } = string.Empty;

    public List<AttributeOptionTranslationItemDto> Translations { get; set; } = new();
}
