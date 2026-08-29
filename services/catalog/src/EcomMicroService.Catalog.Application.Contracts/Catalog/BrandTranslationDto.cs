using System.ComponentModel.DataAnnotations;

namespace EcomMicroService.Catalog;

/// <summary>
/// Translation item for brand localized fields.
/// </summary>
public class BrandTranslationDto : INameDescriptionTranslationDto
{
    [Required]
    public string Language { get; set; } = string.Empty;

    [Required]
    [StringLength(CatalogConsts.Catalog.BrandMaxNameLength)]
    public string Name { get; set; } = string.Empty;

    [StringLength(CatalogConsts.Catalog.BrandMaxDescriptionLength)]
    public string? Description { get; set; }
}
