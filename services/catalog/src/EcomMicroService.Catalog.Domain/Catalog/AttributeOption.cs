using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Catalog;

/// <summary>
/// One invariant allowed value for an enum-typed <see cref="AttributeDefinition"/>.
/// </summary>
public class AttributeOption : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid AttributeDefinitionId { get; private set; }
    public string Value { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }

    public virtual AttributeDefinition? AttributeDefinition { get; set; }

    public virtual ICollection<AttributeOptionTranslation> Translations { get; set; } =
        new List<AttributeOptionTranslation>();

    protected AttributeOption()
    {
    }

    public AttributeOption(
        Guid id,
        Guid attributeDefinitionId,
        string value,
        int displayOrder,
        bool isActive = true)
        : base(id)
    {
        SetAttributeDefinition(attributeDefinitionId);
        SetValue(value);
        SetDisplayOrder(displayOrder);
        IsActive = isActive;
    }

    public void SetAttributeDefinition(Guid attributeDefinitionId)
    {
        if (attributeDefinitionId == Guid.Empty)
        {
            throw new ArgumentException("attributeDefinitionId is required.", nameof(attributeDefinitionId));
        }

        AttributeDefinitionId = attributeDefinitionId;
    }

    public void SetValue(string value)
    {
        Value = NormalizeRequired(value, CatalogConsts.Catalog.AttributeOptionValueMaxLength, nameof(value));
    }

    public void SetDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(displayOrder));
        }

        DisplayOrder = displayOrder;
    }

    public void SetIsActive(bool isActive)
    {
        IsActive = isActive;
    }

    private static string NormalizeRequired(string value, int maxLength, string paramName)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (normalized.Length == 0)
        {
            throw new ArgumentException($"{paramName} is required.", paramName);
        }

        if (normalized.Length > maxLength)
        {
            throw new ArgumentException($"{paramName} exceeds max length of {maxLength}.", paramName);
        }

        return normalized;
    }
}
