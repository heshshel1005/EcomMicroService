using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcomMicroService.Catalog;

/// <summary>
/// DTO for creating a brand.
/// </summary>
public class CreateBrandDto
{
    [Required]
    [StringLength(CatalogConsts.Catalog.BrandMaxNameLength)]
    public string Name { get; set; } = string.Empty;

    [StringLength(CatalogConsts.Catalog.BrandMaxSlugLength)]
    public string? Slug { get; set; }

    [StringLength(CatalogConsts.Catalog.BrandMaxDescriptionLength)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public List<BrandTranslationDto> Translations { get; set; } = new();
}

