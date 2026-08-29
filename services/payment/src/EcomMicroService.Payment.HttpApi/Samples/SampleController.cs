using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace EcomMicroService.Payment.Samples;

[Area(PaymentRemoteServiceConsts.ModuleName)]
[RemoteService(Name = PaymentRemoteServiceConsts.RemoteServiceName)]
[Route("api/payment/sample")]
public class SampleController(ISampleAppService sampleAppService)
    : PaymentController,
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
