using System;
using System.Collections.Generic;

namespace EcomMicroService.Catalog;

public class ProductTypeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<ProductTypeTranslationDto> Translations { get; set; } = new();
}
