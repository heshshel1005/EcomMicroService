using System.ComponentModel.DataAnnotations;

namespace EcomMicroService.Catalog;

/// <summary>
/// Translation item for brand model localized fields.
/// </summary>
public class BrandModelTranslationDto : INameTranslationDto
{
    [Required]
    public string Language { get; set; } = string.Empty;

    [Required]
    [StringLength(CatalogConsts.Catalog.BrandModelMaxNameLength)]
    public string Name { get; set; } = string.Empty;
}
