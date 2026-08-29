using EcomMicroService.Ordering.Localization;
using Volo.Abp.Application.Services;

namespace EcomMicroService.Ordering;

public abstract class OrderingAppService : ApplicationService
{
    protected OrderingAppService()
    {
        LocalizationResource = typeof(OrderingResource);
        ObjectMapperContext = typeof(OrderingApplicationModule);
    }
}
