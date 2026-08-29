using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcomMicroService.Catalog;

/// <summary>
/// DTO for creating a brand model.
/// </summary>
public class CreateBrandModelDto
{
    [Required]
    public Guid BrandId { get; set; }

    [Required]
    [StringLength(CatalogConsts.Catalog.BrandModelMaxNameLength)]
    public string Name { get; set; } = string.Empty;

    [StringLength(CatalogConsts.Catalog.BrandModelMaxCodeLength)]
    public string? Code { get; set; }

    public bool IsActive { get; set; } = true;

    public List<BrandModelTranslationDto> Translations { get; set; } = new();
}

