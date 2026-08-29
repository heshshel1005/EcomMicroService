using System;
using System.Collections.Generic;

namespace EcomMicroService.Catalog;

/// <summary>
/// DTO for a brand model.
/// </summary>
public class BrandModelDto
{
    public Guid Id { get; set; }
    public Guid BrandId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsActive { get; set; }
    public List<BrandModelTranslationDto> Translations { get; set; } = new();
}

