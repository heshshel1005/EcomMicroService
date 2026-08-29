using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcomMicroService.Catalog;

public class CreateProductTypeDto
{
    [Required]
    [StringLength(CatalogConsts.Catalog.ProductTypeMaxCodeLength)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(CatalogConsts.Catalog.ProductTypeMaxNameLength)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public List<ProductTypeTranslationDto> Translations { get; set; } = new();
}
