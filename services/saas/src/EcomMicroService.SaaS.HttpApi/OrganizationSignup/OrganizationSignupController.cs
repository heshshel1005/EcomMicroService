using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

namespace EcomMicroService.SaaS;

[RemoteService(Name = "SaaS")]
[Area("saas")]
[Route("api/saas/organization-signup")]
public class OrganizationSignupController : SaaSController
{
    private readonly IOrganizationSignupPublicAppService _pub;
    private readonly IOrganizationSignupHostAppService _host;

    public OrganizationSignupController(IOrganizationSignupPublicAppService pub, IOrganizationSignupHostAppService host)
    {
        _pub = pub;
        _host = host;
    }

    [AllowAnonymous]
    [HttpPost("submit")]
    public Task<OrganizationSignupSubmitResultDto> SubmitAsync([FromBody] OrganizationSignupSubmitDto input) => _pub.SubmitAsync(input);

    [HttpGet("admin")]
    public Task<PagedResultDto<OrganizationSignupListDto>> GetListAsync([FromQuery] PagedAndSortedResultRequestDto input) => _host.GetListAsync(input);

    [HttpPost("admin/{id}/approve")]
    public Task ApproveAsync(Guid id) => _host.ApproveAsync(id);

    [HttpPost("admin/{id}/reject")]
    public Task RejectAsync(Guid id, [FromQuery] string? reason = null) => _host.RejectAsync(id, reason);
}
