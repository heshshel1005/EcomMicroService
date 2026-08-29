using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace EcomMicroService.Customer;

[Authorize]
public class CustomerProfileAppService : ApplicationService, ICustomerProfileAppService
{
    private readonly IRepository<CustomerProfile, Guid> _profileRepository;
    private readonly IRepository<CustomerAddress, Guid> _addressRepository;

    public CustomerProfileAppService(
        IRepository<CustomerProfile, Guid> profileRepository,
        IRepository<CustomerAddress, Guid> addressRepository)
    {
        _profileRepository = profileRepository;
        _addressRepository = addressRepository;
    }

    public async Task<CustomerProfileDto> GetMyProfileAsync()
    {
        var userId = CurrentUser.Id ?? throw new Volo.Abp.Authorization.AbpAuthorizationException("User must be logged in.");
        var profile = await _profileRepository.FirstOrDefaultAsync(p => p.UserId == userId);
        var displayName = profile?.DisplayName;
        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = CurrentUser.Name ?? CurrentUser.UserName ?? string.Empty;
        }

        return new CustomerProfileDto
        {
            Id = profile?.Id ?? Guid.Empty,
            UserId = userId,
            DisplayName = displayName ?? string.Empty,
            PhoneNumber = profile?.PhoneNumber,
            Email = CurrentUser.Email,
        };
    }

    public async Task<CustomerProfileDto> UpdateMyProfileAsync(UpdateCustomerProfileDto input)
    {
        var userId = CurrentUser.Id ?? throw new Volo.Abp.Authorization.AbpAuthorizationException("User must be logged in.");
        var profile = await _profileRepository.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null)
        {
            profile = new CustomerProfile(GuidGenerator.Create(), userId, input.DisplayName, input.PhoneNumber);
            await _profileRepository.InsertAsync(profile);
        }
        else
        {
            profile.DisplayName = input.DisplayName ?? string.Empty;
            profile.PhoneNumber = string.IsNullOrWhiteSpace(input.PhoneNumber) ? null : input.PhoneNumber.Trim();
            await _profileRepository.UpdateAsync(profile);
        }

        return new CustomerProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            DisplayName = profile.DisplayName,
            PhoneNumber = profile.PhoneNumber,
            Email = CurrentUser.Email,
        };
    }

    public async Task<List<CustomerAddressDto>> GetMyAddressesAsync()
    {
        var userId = CurrentUser.Id ?? throw new Volo.Abp.Authorization.AbpAuthorizationException("User must be logged in.");
        var list = await _addressRepository.GetListAsync(a => a.UserId == userId);
        return list.Select(MapAddress).ToList();
    }

    public async Task<CustomerAddressDto> CreateAddressAsync(CreateUpdateCustomerAddressDto input)
    {
        var userId = CurrentUser.Id ?? throw new Volo.Abp.Authorization.AbpAuthorizationException("User must be logged in.");
        if (input.IsDefaultShipping || input.IsDefaultBilling)
            await ClearDefaultFlagsAsync(userId, input.IsDefaultShipping, input.IsDefaultBilling);
        var entity = new CustomerAddress(
            GuidGenerator.Create(),
            userId,
            input.Label ?? string.Empty,
            input.Street ?? string.Empty,
            input.City,
            input.Region,
            input.PostalCode,
            input.Country,
            input.IsDefaultShipping,
            input.IsDefaultBilling);
        await _addressRepository.InsertAsync(entity);
        return MapAddress(entity);
    }

    public async Task<CustomerAddressDto> UpdateAddressAsync(Guid id, CreateUpdateCustomerAddressDto input)
    {
        var userId = CurrentUser.Id ?? throw new Volo.Abp.Authorization.AbpAuthorizationException("User must be logged in.");
        var entity = await _addressRepository.GetAsync(id);
        if (entity.UserId != userId)
            throw new Volo.Abp.Authorization.AbpAuthorizationException("Not your address.");
        if (input.IsDefaultShipping || input.IsDefaultBilling)
            await ClearDefaultFlagsAsync(userId, input.IsDefaultShipping, input.IsDefaultBilling, excludeId: id);
        entity.Label = input.Label ?? string.Empty;
        entity.Street = input.Street ?? string.Empty;
        entity.City = input.City;
        entity.Region = input.Region;
        entity.PostalCode = input.PostalCode;
        entity.Country = input.Country;
        entity.IsDefaultShipping = input.IsDefaultShipping;
        entity.IsDefaultBilling = input.IsDefaultBilling;
        await _addressRepository.UpdateAsync(entity);
        return MapAddress(entity);
    }

    public async Task DeleteAddressAsync(Guid id)
    {
        var userId = CurrentUser.Id ?? throw new Volo.Abp.Authorization.AbpAuthorizationException("User must be logged in.");
        var entity = await _addressRepository.GetAsync(id);
        if (entity.UserId != userId)
            throw new Volo.Abp.Authorization.AbpAuthorizationException("Not your address.");
        await _addressRepository.DeleteAsync(entity);
    }

    private static CustomerAddressDto MapAddress(CustomerAddress a) => new()
    {
        Id = a.Id,
        UserId = a.UserId,
        Label = a.Label,
        Street = a.Street,
        City = a.City,
        Region = a.Region,
        PostalCode = a.PostalCode,
        Country = a.Country,
        IsDefaultShipping = a.IsDefaultShipping,
        IsDefaultBilling = a.IsDefaultBilling,
    };

    private async Task ClearDefaultFlagsAsync(Guid userId, bool clearShipping, bool clearBilling, Guid? excludeId = null)
    {
        var addresses = await _addressRepository.GetListAsync(a => a.UserId == userId);
        foreach (var a in addresses)
        {
            if (excludeId.HasValue && a.Id == excludeId.Value) continue;
            var changed = false;
            if (clearShipping && a.IsDefaultShipping) { a.IsDefaultShipping = false; changed = true; }
            if (clearBilling && a.IsDefaultBilling) { a.IsDefaultBilling = false; changed = true; }
            if (changed) await _addressRepository.UpdateAsync(a);
        }
    }
}
