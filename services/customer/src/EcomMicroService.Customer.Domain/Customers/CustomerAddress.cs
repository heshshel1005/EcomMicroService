using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Customer;

public class CustomerAddress : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid UserId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public bool IsDefaultShipping { get; set; }
    public bool IsDefaultBilling { get; set; }

    protected CustomerAddress()
    {
    }

    public CustomerAddress(
        Guid id,
        Guid userId,
        string label,
        string street,
        string? city = null,
        string? region = null,
        string? postalCode = null,
        string? country = null,
        bool isDefaultShipping = false,
        bool isDefaultBilling = false)
        : base(id)
    {
        UserId = userId;
        Label = label ?? string.Empty;
        Street = street ?? string.Empty;
        City = city;
        Region = region;
        PostalCode = postalCode;
        Country = country;
        IsDefaultShipping = isDefaultShipping;
        IsDefaultBilling = isDefaultBilling;
    }
}
