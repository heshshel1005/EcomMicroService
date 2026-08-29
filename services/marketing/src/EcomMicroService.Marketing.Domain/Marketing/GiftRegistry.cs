using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Marketing;

/// <summary>
/// Gift registry: owner (user), title, optional slug for public URL, event date.
/// </summary>
public class GiftRegistry : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid OwnerUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    /// <summary>Unique slug for public URL (e.g. /registry/jane-wedding).</summary>
    public string Slug { get; set; } = string.Empty;
    public DateTime? EventDate { get; set; }
    public bool IsPublic { get; set; } = true;

    protected GiftRegistry()
    {
    }

    public GiftRegistry(Guid id, Guid ownerUserId, string title, string slug, DateTime? eventDate = null)
        : base(id)
    {
        OwnerUserId = ownerUserId;
        Title = title ?? string.Empty;
        Slug = slug?.Trim().ToLowerInvariant() ?? string.Empty;
        EventDate = eventDate;
    }
}

