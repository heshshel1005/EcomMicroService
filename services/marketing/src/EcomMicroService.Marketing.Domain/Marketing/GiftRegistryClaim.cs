using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Marketing;

/// <summary>
/// A claim on a registry item: a guest reserves or purchases quantity; optional message.
/// </summary>
public class GiftRegistryClaim : AuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid GiftRegistryItemId { get; set; }
    public int Quantity { get; set; }
    /// <summary>Guest identifier: UserId if logged in, else null (anonymous claim).</summary>
    public Guid? ClaimedByUserId { get; set; }
    public string? ClaimantName { get; set; }
    public string? Message { get; set; }
    /// <summary>True when the claim has been fulfilled (e.g. order placed).</summary>
    public bool IsFulfilled { get; set; }
    public Guid? OrderId { get; set; }

    protected GiftRegistryClaim()
    {
    }

    public GiftRegistryClaim(Guid id, Guid giftRegistryItemId, int quantity, Guid? claimedByUserId = null, string? claimantName = null, string? message = null)
        : base(id)
    {
        GiftRegistryItemId = giftRegistryItemId;
        Quantity = quantity;
        ClaimedByUserId = claimedByUserId;
        ClaimantName = claimantName;
        Message = message;
    }
}

