using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace EcomMicroService.Customer;

[RemoteService(Name = "Customer")]
[Area("customer")]
[Route("api/customer/profile")]
public class CustomerProfileController : CustomerController, ICustomerProfileAppService
{
    private readonly ICustomerProfileAppService _service;

    public CustomerProfileController(ICustomerProfileAppService service)
    {
        _service = service;
    }

    [HttpGet]
    public Task<CustomerProfileDto> GetMyProfileAsync() => _service.GetMyProfileAsync();

    [HttpPut]
    public Task<CustomerProfileDto> UpdateMyProfileAsync(UpdateCustomerProfileDto input) =>
        _service.UpdateMyProfileAsync(input);

    [HttpGet("addresses")]
    public Task<List<CustomerAddressDto>> GetMyAddressesAsync() => _service.GetMyAddressesAsync();

    [HttpPost("addresses")]
    public Task<CustomerAddressDto> CreateAddressAsync(CreateUpdateCustomerAddressDto input) =>
        _service.CreateAddressAsync(input);

    [HttpPut("addresses/{id}")]
    public Task<CustomerAddressDto> UpdateAddressAsync(Guid id, CreateUpdateCustomerAddressDto input) =>
        _service.UpdateAddressAsync(id, input);

    [HttpDelete("addresses/{id}")]
    public Task DeleteAddressAsync(Guid id) => _service.DeleteAddressAsync(id);
}
