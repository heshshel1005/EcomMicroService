using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace EcomMicroService.Basket.Baskets;

[RemoteService(Name = "Basket")]
[Area("basket")]
[Route("api/basket")]
public class CartController : BasketController, IBasketAppService
{
    private readonly IBasketAppService _basketAppService;

    public CartController(IBasketAppService basketAppService)
    {
        _basketAppService = basketAppService;
    }

    [HttpGet]
    public Task<BasketDto> GetAsync(Guid? anonymousId = null)
    {
        return _basketAppService.GetAsync(anonymousId);
    }

    [HttpPut]
    public Task<BasketDto> UpdateAsync(BasketDto input, Guid? anonymousId = null)
    {
        return _basketAppService.UpdateAsync(input, anonymousId);
    }

    [HttpDelete]
    public Task ClearAsync(Guid? anonymousId = null)
    {
        return _basketAppService.ClearAsync(anonymousId);
    }
}
