using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace EcomMicroService.Ordering.Samples;

[Area(OrderingRemoteServiceConsts.ModuleName)]
[RemoteService(Name = OrderingRemoteServiceConsts.RemoteServiceName)]
[Route("api/Ordering/sample")]
public class SampleController(ISampleAppService sampleAppService)
    : OrderingController,
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
