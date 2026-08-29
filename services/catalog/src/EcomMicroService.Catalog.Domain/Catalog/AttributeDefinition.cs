using System;
using System.Collections.Generic;
using EcomMicroService.Catalog.Events;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Catalog;

/// <summary>
/// Defines a dynamic catalog attribute (e.g. part_number, condition, gtin_upc); host-shared definitions use <see cref="TenantId"/> null.
/// Rules are stored on the definition and evaluated when products are created or updated.
/// </summary>
public class AttributeDefinition : AuditedAggregateRoot<Guid>, IMultiTenant, ICatalogTaxonomyEntity
{
    public Guid? TenantId { get; set; }
    public string Key { get; private set; } = string.Empty;
    public AttributeDefinitionDataType DataType { get; private set; }
    public string? AllowedValuesJson { get; private set; }
    public string? RegexPattern { get; private set; }
    public decimal? MinValue { get; private set; }
    public decimal? MaxValue { get; private set; }
    public bool IsRequired { get; private set; }
    public bool IsRecommended { get; private set; }

    /// <summary>Approval / publication lifecycle; only <see cref="AttributeDefinitionGovernanceStatus.Published"/> participates in storefront validation.</summary>
    public AttributeDefinitionGovernanceStatus GovernanceStatus { get; private set; }

    /// <summary>Increments on each successful <see cref="Publish"/> (governance version, not a separate aggregate row).</summary>
    public int PublishedVersion { get; private set; }

    public virtual ICollection<AttributeDefinitionTranslation> Translations { get; set; } =
        new List<AttributeDefinitionTranslation>();

    public virtual ICollection<AttributeOption> Options { get; set; } =
        new List<AttributeOption>();

    protected AttributeDefinition()
    {
    }

    public AttributeDefinition(
        Guid id,
        string key,
        AttributeDefinitionDataType dataType,
        string? allowedValuesJson = null,
        string? regexPattern = null,
        decimal? minValue = null,
        decimal? maxValue = null,
        bool isRequired = false,
        bool isRecommended = false)
        : base(id)
    {
        SetKey(key);
        SetDataType(dataType);
        SetAllowedValuesJson(allowedValuesJson);
        SetRegexPattern(regexPattern);
        SetRange(minValue, maxValue);
        SetRequirementFlags(isRequired, isRecommended);
        GovernanceStatus = AttributeDefinitionGovernanceStatus.Draft;
        PublishedVersion = 0;
    }

    public void SetKey(string key)
    {
        Key = NormalizeRequired(key, nameof(key));
    }

    public void SetDataType(AttributeDefinitionDataType dataType)
    {
        DataType = dataType;
    }

    public void SetAllowedValuesJson(string? allowedValuesJson)
    {
        AllowedValuesJson = NormalizeOptional(allowedValuesJson);
    }

    public void SetRegexPattern(string? regexPattern)
    {
        RegexPattern = NormalizeOptional(regexPattern);
    }

    public void SetRange(decimal? minValue, decimal? maxValue)
    {
        if (minValue.HasValue && maxValue.HasValue && minValue.Value > maxValue.Value)
        {
            throw new ArgumentException("minValue cannot be greater than maxValue.");
        }

        MinValue = minValue;
        MaxValue = maxValue;
    }

    public void SetRequirementFlags(bool isRequired, bool isRecommended)
    {
        IsRequired = isRequired;
        IsRecommended = isRecommended;
    }

    public void SubmitForReview()
    {
        EnsureNotArchivedForMutation();
        if (GovernanceStatus != AttributeDefinitionGovernanceStatus.Draft)
        {
            throw new BusinessException(CatalogDomainErrorCodes.AttributeDefinitionInvalidGovernanceTransition)
                .WithData("From", GovernanceStatus.ToString())
                .WithData("To", nameof(AttributeDefinitionGovernanceStatus.PendingReview));
        }

        GovernanceStatus = AttributeDefinitionGovernanceStatus.PendingReview;
        AddLocalEvent(new AttributeDefinitionSubmittedForReviewEvent(Id, TenantId));
    }

    public void RejectReview()
    {
        EnsureNotArchivedForMutation();
        if (GovernanceStatus != AttributeDefinitionGovernanceStatus.PendingReview)
        {
            throw new BusinessException(CatalogDomainErrorCodes.AttributeDefinitionInvalidGovernanceTransition)
                .WithData("From", GovernanceStatus.ToString())
                .WithData("To", nameof(AttributeDefinitionGovernanceStatus.Draft));
        }

        GovernanceStatus = AttributeDefinitionGovernanceStatus.Draft;
        AddLocalEvent(new AttributeDefinitionReviewRejectedEvent(Id, TenantId));
    }

    /// <summary>Publishes from draft or pending review, or re-publishes (bumps <see cref="PublishedVersion"/>).</summary>
    public void Publish()
    {
        EnsureNotArchivedForMutation();
        if (GovernanceStatus == AttributeDefinitionGovernanceStatus.PendingReview ||
            GovernanceStatus == AttributeDefinitionGovernanceStatus.Draft)
        {
            GovernanceStatus = AttributeDefinitionGovernanceStatus.Published;
        }
        else if (GovernanceStatus != AttributeDefinitionGovernanceStatus.Published)
        {
            throw new BusinessException(CatalogDomainErrorCodes.AttributeDefinitionInvalidGovernanceTransition)
                .WithData("From", GovernanceStatus.ToString())
                .WithData("To", nameof(AttributeDefinitionGovernanceStatus.Published));
        }

        PublishedVersion += 1;
        AddLocalEvent(new AttributeDefinitionPublishedEvent(Id, TenantId, PublishedVersion));
    }

    public void Archive()
    {
        if (GovernanceStatus == AttributeDefinitionGovernanceStatus.Archived)
        {
            return;
        }

        GovernanceStatus = AttributeDefinitionGovernanceStatus.Archived;
        AddLocalEvent(new AttributeDefinitionArchivedEvent(Id, TenantId));
    }

    /// <summary>Returns a published definition to draft for further edits and re-approval.</summary>
    public void DemoteToDraft()
    {
        if (GovernanceStatus == AttributeDefinitionGovernanceStatus.Archived)
        {
            throw new BusinessException(CatalogDomainErrorCodes.AttributeDefinitionInvalidGovernanceTransition)
                .WithData("From", GovernanceStatus.ToString())
                .WithData("To", nameof(AttributeDefinitionGovernanceStatus.Draft));
        }

        if (GovernanceStatus == AttributeDefinitionGovernanceStatus.Draft)
        {
            return;
        }

        GovernanceStatus = AttributeDefinitionGovernanceStatus.Draft;
        AddLocalEvent(new AttributeDefinitionDemotedToDraftEvent(Id, TenantId));
    }

    private void EnsureNotArchivedForMutation()
    {
        if (GovernanceStatus == AttributeDefinitionGovernanceStatus.Archived)
        {
            throw new BusinessException(CatalogDomainErrorCodes.AttributeDefinitionArchivedMutationBlocked);
        }
    }

    private static string NormalizeRequired(string value, string paramName)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (normalized.Length == 0)
        {
            throw new ArgumentException($"{paramName} is required.", paramName);
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}
