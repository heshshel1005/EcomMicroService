using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Marketing;

/// <summary>
/// Newsletter subscription: email and optional name for campaign list.
/// </summary>
public class NewsletterSubscriber : CreationAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? UnsubscribedAt { get; set; }

    protected NewsletterSubscriber()
    {
    }

    public NewsletterSubscriber(Guid id, string email, string? name = null)
        : base(id)
    {
        Email = email?.Trim().ToLowerInvariant() ?? string.Empty;
        Name = name?.Trim();
    }

    public void Unsubscribe()
    {
        IsActive = false;
        UnsubscribedAt = DateTime.UtcNow;
    }

    public void Resubscribe()
    {
        IsActive = true;
        UnsubscribedAt = null;
    }
}

