using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Catalog;

/// <summary>
/// Maps a product type to an attribute definition and stores presentation/validation metadata.
/// </summary>
public class ProductTypeAttributeRule : AuditedAggregateRoot<Guid>, IMultiTenant, ICatalogTaxonomyEntity
{
    public Guid? TenantId { get; set; }
    public Guid ProductTypeId { get; private set; }
    public Guid AttributeDefinitionId { get; private set; }
    public int DisplayOrder { get; private set; }
    public string? ConditionalAttributeKey { get; private set; }
    public ProductTypeRuleConditionOperator? ConditionalOperator { get; private set; }
    public string? ConditionalExpectedValue { get; private set; }

    protected ProductTypeAttributeRule()
    {
    }

    public ProductTypeAttributeRule(
        Guid id,
        Guid productTypeId,
        Guid attributeDefinitionId,
        int displayOrder = 0,
        string? conditionalAttributeKey = null,
        ProductTypeRuleConditionOperator? conditionalOperator = null,
        string? conditionalExpectedValue = null)
        : base(id)
    {
        SetProductType(productTypeId);
        SetAttributeDefinition(attributeDefinitionId);
        SetDisplayOrder(displayOrder);
        SetConditionalMetadata(conditionalAttributeKey, conditionalOperator, conditionalExpectedValue);
    }

    public void SetProductType(Guid productTypeId)
    {
        if (productTypeId == Guid.Empty)
        {
            throw new ArgumentException("productTypeId is required.", nameof(productTypeId));
        }

        ProductTypeId = productTypeId;
    }

    public void SetAttributeDefinition(Guid attributeDefinitionId)
    {
        if (attributeDefinitionId == Guid.Empty)
        {
            throw new ArgumentException("attributeDefinitionId is required.", nameof(attributeDefinitionId));
        }

        AttributeDefinitionId = attributeDefinitionId;
    }

    public void SetDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(displayOrder), "displayOrder cannot be negative.");
        }

        DisplayOrder = displayOrder;
    }

    public void SetConditionalMetadata(
        string? conditionalAttributeKey,
        ProductTypeRuleConditionOperator? conditionalOperator,
        string? conditionalExpectedValue)
    {
        var normalizedKey = NormalizeOptional(conditionalAttributeKey, CatalogConsts.Catalog.ProductTypeRuleMaxConditionAttributeKeyLength, nameof(conditionalAttributeKey));
        var normalizedExpectedValue = NormalizeOptional(conditionalExpectedValue, CatalogConsts.Catalog.ProductTypeRuleMaxConditionExpectedValueLength, nameof(conditionalExpectedValue));

        var hasAnyConditionPart = normalizedKey is not null || conditionalOperator.HasValue || normalizedExpectedValue is not null;
        if (hasAnyConditionPart)
        {
            if (normalizedKey is null)
            {
                throw new ArgumentException("conditionalAttributeKey is required when condition metadata is set.", nameof(conditionalAttributeKey));
            }

            if (!conditionalOperator.HasValue)
            {
                throw new ArgumentException("conditionalOperator is required when condition metadata is set.", nameof(conditionalOperator));
            }
        }

        ConditionalAttributeKey = normalizedKey;
        ConditionalOperator = conditionalOperator;
        ConditionalExpectedValue = normalizedExpectedValue;
    }

    private static string? NormalizeOptional(string? value, int maxLength, string paramName)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        if (normalized.Length > maxLength)
        {
            throw new ArgumentException($"{paramName} exceeds max length of {maxLength}.", paramName);
        }

        return normalized;
    }
}
