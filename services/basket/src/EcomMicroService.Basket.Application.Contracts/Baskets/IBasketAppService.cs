using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace EcomMicroService.Basket.Baskets;

public interface IBasketAppService : IApplicationService
{
    Task<BasketDto> GetAsync(Guid? anonymousId = null);
    Task<BasketDto> UpdateAsync(BasketDto input, Guid? anonymousId = null);
    Task ClearAsync(Guid? anonymousId = null);
}
