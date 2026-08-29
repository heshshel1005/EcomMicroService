using System;

namespace EcomMicroService.Catalog;

public class ProductTypeAttributeRuleDto
{
    public Guid Id { get; set; }
    public Guid ProductTypeId { get; set; }
    public Guid AttributeDefinitionId { get; set; }
    public int DisplayOrder { get; set; }
    public string? ConditionalAttributeKey { get; set; }
    public ProductTypeRuleConditionOperator? ConditionalOperator { get; set; }
    public string? ConditionalExpectedValue { get; set; }
}
