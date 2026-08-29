using System.ComponentModel.DataAnnotations;

namespace EcomMicroService.Catalog;

/// <summary>
/// Translation item for category localized fields.
/// </summary>
public class CategoryTranslationDto : INameTranslationDto
{
    [Required]
    public string Language { get; set; } = string.Empty;

    [Required]
    [StringLength(CatalogConsts.Catalog.CategoryMaxNameLength)]
    public string Name { get; set; } = string.Empty;
}
