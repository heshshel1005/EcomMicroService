using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace EcomMicroService.Customer.Samples;

[Area(CustomerRemoteServiceConsts.ModuleName)]
[RemoteService(Name = CustomerRemoteServiceConsts.RemoteServiceName)]
[Route("api/customer/sample")]
public class SampleController(ISampleAppService sampleAppService)
    : CustomerController,
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
