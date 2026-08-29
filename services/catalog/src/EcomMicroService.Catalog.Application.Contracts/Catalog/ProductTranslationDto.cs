using System.ComponentModel.DataAnnotations;

namespace EcomMicroService.Catalog;

/// <summary>
/// Translation item for product localized fields.
/// </summary>
public class ProductTranslationDto : INameDescriptionTranslationDto
{
    [Required]
    public string Language { get; set; } = string.Empty;

    [Required]
    [StringLength(CatalogConsts.Catalog.ProductMaxNameLength)]
    public string Name { get; set; } = string.Empty;

    [StringLength(CatalogConsts.Catalog.ProductMaxDescriptionLength)]
    public string? Description { get; set; }
}
