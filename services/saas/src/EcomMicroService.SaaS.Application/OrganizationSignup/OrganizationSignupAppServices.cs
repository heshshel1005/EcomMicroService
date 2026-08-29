using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Security.Encryption;
using Volo.Abp.TenantManagement;

namespace EcomMicroService.SaaS;

public class OrganizationSignupPublicAppService : ApplicationService, IOrganizationSignupPublicAppService
{
    private readonly IRepository<OrganizationSignupRequest, Guid> _signups;
    private readonly IStringEncryptionService _encryption;

    public OrganizationSignupPublicAppService(
        IRepository<OrganizationSignupRequest, Guid> signups,
        IStringEncryptionService encryption)
    {
        _signups = signups;
        _encryption = encryption;
    }

    [AllowAnonymous]
    public async Task<OrganizationSignupSubmitResultDto> SubmitAsync(OrganizationSignupSubmitDto input)
    {
        var cipher = _encryption.Encrypt(input.AdminPassword);
        var entity = new OrganizationSignupRequest(
            GuidGenerator.Create(),
            input.TenantName,
            input.DisplayName,
            input.BusinessType,
            input.AdminEmail,
            input.AdminUserName,
            input.AdminDisplayName,
            cipher ?? string.Empty)
        {
            LegalName = input.LegalName,
            Website = input.Website,
            Phone = input.Phone,
            ShortDescription = input.ShortDescription
        };
        await _signups.InsertAsync(entity);
        return new OrganizationSignupSubmitResultDto
        {
            RequestId = entity.Id,
            Message = "Your organization signup was received and is pending review."
        };
    }
}

[Authorize("ECommerce.TenantSignup.Manage")]
public class OrganizationSignupHostAppService : ApplicationService, IOrganizationSignupHostAppService
{
    private readonly IRepository<OrganizationSignupRequest, Guid> _signups;
    private readonly ITenantManager _tenantManager;
    private readonly IRepository<Tenant, Guid> _tenants;

    public OrganizationSignupHostAppService(
        IRepository<OrganizationSignupRequest, Guid> signups,
        ITenantManager tenantManager,
        IRepository<Tenant, Guid> tenants)
    {
        _signups = signups;
        _tenantManager = tenantManager;
        _tenants = tenants;
    }

    public async Task<PagedResultDto<OrganizationSignupListDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var query = await _signups.GetQueryableAsync();
        var total = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount > 0 ? input.MaxResultCount : 20));
        return new PagedResultDto<OrganizationSignupListDto>(total, items.Select(x => new OrganizationSignupListDto
        {
            Id = x.Id,
            TenantName = x.TenantName,
            DisplayName = x.DisplayName,
            AdminEmail = x.AdminEmail,
            Status = x.Status,
            CreationTime = x.CreationTime
        }).ToList());
    }

    public async Task ApproveAsync(Guid id)
    {
        var req = await _signups.GetAsync(id);
        var tenant = await _tenantManager.CreateAsync(req.TenantName);
        await _tenants.InsertAsync(tenant);
        req.Status = OrganizationSignupStatus.Approved;
        req.CreatedTenantId = tenant.Id;
        req.ReviewedTime = DateTime.UtcNow;
        req.ReviewerUserId = CurrentUser.Id;
        await _signups.UpdateAsync(req);
    }

    public async Task RejectAsync(Guid id, string? reason = null)
    {
        var req = await _signups.GetAsync(id);
        req.Status = OrganizationSignupStatus.Rejected;
        req.RejectionReason = reason;
        req.ReviewedTime = DateTime.UtcNow;
        req.ReviewerUserId = CurrentUser.Id;
        await _signups.UpdateAsync(req);
    }
}
