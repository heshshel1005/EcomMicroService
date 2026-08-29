using System;

namespace EcomMicroService.Catalog.Events;

public class AttributeDefinitionSubmittedForReviewEvent
{
    public Guid AttributeDefinitionId { get; }
    public Guid? TenantId { get; }

    public AttributeDefinitionSubmittedForReviewEvent(Guid attributeDefinitionId, Guid? tenantId)
    {
        AttributeDefinitionId = attributeDefinitionId;
        TenantId = tenantId;
    }
}

public class AttributeDefinitionReviewRejectedEvent
{
    public Guid AttributeDefinitionId { get; }
    public Guid? TenantId { get; }

    public AttributeDefinitionReviewRejectedEvent(Guid attributeDefinitionId, Guid? tenantId)
    {
        AttributeDefinitionId = attributeDefinitionId;
        TenantId = tenantId;
    }
}

public class AttributeDefinitionPublishedEvent
{
    public Guid AttributeDefinitionId { get; }
    public Guid? TenantId { get; }
    public int PublishedVersion { get; }

    public AttributeDefinitionPublishedEvent(Guid attributeDefinitionId, Guid? tenantId, int publishedVersion)
    {
        AttributeDefinitionId = attributeDefinitionId;
        TenantId = tenantId;
        PublishedVersion = publishedVersion;
    }
}

public class AttributeDefinitionArchivedEvent
{
    public Guid AttributeDefinitionId { get; }
    public Guid? TenantId { get; }

    public AttributeDefinitionArchivedEvent(Guid attributeDefinitionId, Guid? tenantId)
    {
        AttributeDefinitionId = attributeDefinitionId;
        TenantId = tenantId;
    }
}

public class AttributeDefinitionDemotedToDraftEvent
{
    public Guid AttributeDefinitionId { get; }
    public Guid? TenantId { get; }

    public AttributeDefinitionDemotedToDraftEvent(Guid attributeDefinitionId, Guid? tenantId)
    {
        AttributeDefinitionId = attributeDefinitionId;
        TenantId = tenantId;
    }
}
