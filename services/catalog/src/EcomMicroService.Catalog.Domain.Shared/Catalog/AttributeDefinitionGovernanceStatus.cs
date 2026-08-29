namespace EcomMicroService.Catalog;

/// <summary>
/// Lifecycle for attribute definitions: draft work, optional review, publication to the catalog, or archival.
/// </summary>
public enum AttributeDefinitionGovernanceStatus
{
    Draft = 0,
    PendingReview = 1,
    Published = 2,
    Archived = 3
}
