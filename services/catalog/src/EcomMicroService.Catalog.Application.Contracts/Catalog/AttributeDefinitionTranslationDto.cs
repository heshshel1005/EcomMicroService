using System.ComponentModel.DataAnnotations;

namespace EcomMicroService.Catalog;

/// <summary>
/// Translation item for <see cref="AttributeDefinition"/> localized display name and description.
/// </summary>
public class AttributeDefinitionTranslationDto : INameDescriptionTranslationDto
{
    [Required]
    [StringLength(CatalogConsts.Catalog.TranslationLanguageMaxLength)]
    public string Language { get; set; } = string.Empty;

    [Required]
    [StringLength(CatalogConsts.Catalog.AttributeDefinitionTranslationDisplayNameMaxLength)]
    public string Name { get; set; } = string.Empty;

    [StringLength(CatalogConsts.Catalog.AttributeDefinitionTranslationDescriptionMaxLength)]
    public string? Description { get; set; }
}
