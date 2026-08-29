using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Customer;

public class CustomerProfile : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }

    protected CustomerProfile()
    {
    }

    public CustomerProfile(Guid id, Guid userId, string displayName, string? phoneNumber = null)
        : base(id)
    {
        UserId = userId;
        DisplayName = displayName ?? string.Empty;
        PhoneNumber = phoneNumber;
    }
}
