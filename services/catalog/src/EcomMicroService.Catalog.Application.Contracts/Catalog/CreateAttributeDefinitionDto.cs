using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcomMicroService.Catalog;

public class CreateAttributeDefinitionDto
{
    [Required]
    [StringLength(CatalogConsts.Catalog.ProductAttributeMaxNameLength)]
    public string Key { get; set; } = string.Empty;

    public AttributeDefinitionDataType DataType { get; set; } = AttributeDefinitionDataType.Text;
    public string? AllowedValuesJson { get; set; }
    public string? RegexPattern { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public bool IsRequired { get; set; }
    public bool IsRecommended { get; set; }

    /// <summary>
    /// At least one entry is required; must include the tenant default language (validated in the application layer).
    /// </summary>
    [Required]
    [MinLength(1)]
    public List<AttributeDefinitionTranslationDto> Translations { get; set; } = new();
}
