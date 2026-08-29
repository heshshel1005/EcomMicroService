using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Caching;
using Volo.Abp.Users;

namespace EcomMicroService.Basket.Baskets;

public class BasketAppService : ApplicationService, IBasketAppService
{
    private readonly IDistributedCache<BasketDto> _basketCache;
    private readonly ICurrentUser _currentUser;

    public BasketAppService(
        IDistributedCache<BasketDto> basketCache,
        ICurrentUser currentUser)
    {
        _basketCache = basketCache;
        _currentUser = currentUser;
    }

    public async Task<BasketDto> GetAsync(Guid? anonymousId = null)
    {
        var cacheKey = GetCacheKey(anonymousId);
        if (string.IsNullOrWhiteSpace(cacheKey))
        {
            return new BasketDto();
        }

        var basket = await _basketCache.GetAsync(cacheKey);
        return basket ?? new BasketDto();
    }

    public async Task<BasketDto> UpdateAsync(BasketDto input, Guid? anonymousId = null)
    {
        var cacheKey = GetCacheKey(anonymousId);
        if (string.IsNullOrWhiteSpace(cacheKey))
        {
            throw new ArgumentException("User is not authenticated and no anonymous cart identifier is provided.");
        }

        await _basketCache.SetAsync(cacheKey, input);
        return input;
    }

    public async Task ClearAsync(Guid? anonymousId = null)
    {
        var cacheKey = GetCacheKey(anonymousId);
        if (!string.IsNullOrWhiteSpace(cacheKey))
        {
            await _basketCache.RemoveAsync(cacheKey);
        }
    }

    private string GetCacheKey(Guid? anonymousId)
    {
        if (_currentUser.IsAuthenticated)
        {
            return _currentUser.Id.ToString();
        }

        return anonymousId?.ToString();
    }
}
