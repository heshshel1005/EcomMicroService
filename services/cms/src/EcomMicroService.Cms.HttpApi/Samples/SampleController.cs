using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace EcomMicroService.Cms.Samples;

[Area(CmsRemoteServiceConsts.ModuleName)]
[RemoteService(Name = CmsRemoteServiceConsts.RemoteServiceName)]
[Route("api/cms/sample")]
public class SampleController(ISampleAppService sampleAppService)
    : CmsController,
        ISampleAppService
{
    private readonly ISampleAppService _sampleAppService = sampleAppService;

    [HttpGet]
    public async Task<SampleDto> GetAsync()
    {
        return await _sampleAppService.GetAsync();
    }

    [HttpGet]
    [Route("authorized")]
    [Authorize]
    public async Task<SampleDto> GetAuthorizedAsync()
    {
        return await _sampleAppService.GetAsync();
    }
}
