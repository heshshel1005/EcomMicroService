using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Identity;

namespace EcomMicroService.IdentityService;

[RemoteService(Name = "IdentityService")]
[Area("account")]
[Route("api/account")]
public class ShopAccountController : IdentityServiceController
{
    private readonly IShopAccountAppService _app;

    public ShopAccountController(IShopAccountAppService app)
    {
        _app = app;
    }

    [AllowAnonymous]
    [HttpPost("subscribe")]
    public Task<IdentityUserDto> SubscribeAsync([FromBody] CustomerRegisterDto input) => _app.SubscribeAsync(input);
}
