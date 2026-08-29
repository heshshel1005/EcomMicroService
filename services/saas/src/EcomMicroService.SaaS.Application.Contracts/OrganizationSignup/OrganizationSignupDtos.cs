using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EcomMicroService.SaaS;

public class OrganizationSignupSubmitDto
{
    [Required] public string TenantName { get; set; } = string.Empty;
    [Required] public string DisplayName { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public OrganizationBusinessType BusinessType { get; set; }
    public string? Website { get; set; }
    public string? Phone { get; set; }
    public string? ShortDescription { get; set; }
    [Required][EmailAddress] public string AdminEmail { get; set; } = string.Empty;
    [Required] public string AdminUserName { get; set; } = string.Empty;
    [Required] public string AdminDisplayName { get; set; } = string.Empty;
    [Required] public string AdminPassword { get; set; } = string.Empty;
}

public class OrganizationSignupSubmitResultDto
{
    public Guid RequestId { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class OrganizationSignupListDto : EntityDto<Guid>
{
    public string TenantName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public OrganizationSignupStatus Status { get; set; }
    public DateTime CreationTime { get; set; }
}

public interface IOrganizationSignupPublicAppService : IApplicationService
{
    Task<OrganizationSignupSubmitResultDto> SubmitAsync(OrganizationSignupSubmitDto input);
}

public interface IOrganizationSignupHostAppService : IApplicationService
{
    Task<PagedResultDto<OrganizationSignupListDto>> GetListAsync(PagedAndSortedResultRequestDto input);
    Task ApproveAsync(Guid id);
    Task RejectAsync(Guid id, string? reason = null);
}
