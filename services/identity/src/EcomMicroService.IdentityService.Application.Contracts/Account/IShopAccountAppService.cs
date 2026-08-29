using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Account;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;

namespace EcomMicroService.IdentityService;

public class CustomerRegisterDto : RegisterDto
{
    [StringLength(256)]
    public string? Name { get; set; }

    [StringLength(32)]
    public string? PhoneNumber { get; set; }
}

public interface IShopAccountAppService : IApplicationService
{
    Task<IdentityUserDto> SubscribeAsync(CustomerRegisterDto input);
}
