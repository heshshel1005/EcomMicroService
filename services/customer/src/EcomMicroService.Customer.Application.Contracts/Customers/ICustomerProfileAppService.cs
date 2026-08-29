using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EcomMicroService.Customer;

public interface ICustomerProfileAppService : IApplicationService
{
    Task<CustomerProfileDto> GetMyProfileAsync();
    Task<CustomerProfileDto> UpdateMyProfileAsync(UpdateCustomerProfileDto input);
    Task<List<CustomerAddressDto>> GetMyAddressesAsync();
    Task<CustomerAddressDto> CreateAddressAsync(CreateUpdateCustomerAddressDto input);
    Task<CustomerAddressDto> UpdateAddressAsync(Guid id, CreateUpdateCustomerAddressDto input);
    Task DeleteAddressAsync(Guid id);
}

public class CustomerProfileDto : EntityDto<Guid>
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}

public class CustomerAddressDto : EntityDto<Guid>
{
    public Guid UserId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public bool IsDefaultShipping { get; set; }
    public bool IsDefaultBilling { get; set; }
}

public class UpdateCustomerProfileDto
{
    public string DisplayName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}

public class CreateUpdateCustomerAddressDto
{
    public string Label { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public bool IsDefaultShipping { get; set; }
    public bool IsDefaultBilling { get; set; }
}
