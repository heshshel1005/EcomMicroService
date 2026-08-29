using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Marketing;

/// <summary>
/// Loyalty points balance per customer (user). One record per user.
/// </summary>
public class CustomerPoints : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid UserId { get; set; }
    /// <summary>Current points balance (can be increased by earn, decreased by redemption).</summary>
    public int Balance { get; set; }
    /// <summary>Optional tier name for future use (e.g. Silver, Gold).</summary>
    public string? Tier { get; set; }

    protected CustomerPoints()
    {
    }

    public CustomerPoints(Guid id, Guid userId, int balance = 0, string? tier = null)
        : base(id)
    {
        UserId = userId;
        Balance = balance;
        Tier = tier;
    }

    public void AddPoints(int amount)
    {
        if (amount <= 0) return;
        Balance += amount;
    }

    public void DeductPoints(int amount)
    {
        if (amount <= 0) return;
        if (amount > Balance)
            throw new InvalidOperationException("Insufficient points balance.");
        Balance -= amount;
    }
}

