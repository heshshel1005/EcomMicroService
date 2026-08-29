using System.ComponentModel.DataAnnotations;

namespace EcomMicroService.Catalog;

public class ProductTypeTranslationDto
{
    [Required]
    public string Language { get; set; } = string.Empty;

    [Required]
    [StringLength(CatalogConsts.Catalog.ProductTypeMaxNameLength)]
    public string Name { get; set; } = string.Empty;
}
