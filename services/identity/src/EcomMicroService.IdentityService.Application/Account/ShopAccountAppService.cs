using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Account;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;

namespace EcomMicroService.IdentityService;

public class ShopAccountAppService : ApplicationService, IShopAccountAppService
{
    private readonly IAccountAppService _accountAppService;

    public ShopAccountAppService(IAccountAppService accountAppService)
    {
        _accountAppService = accountAppService;
    }

    [AllowAnonymous]
    public Task<IdentityUserDto> SubscribeAsync(CustomerRegisterDto input)
    {
        return _accountAppService.RegisterAsync(input);
    }
}
