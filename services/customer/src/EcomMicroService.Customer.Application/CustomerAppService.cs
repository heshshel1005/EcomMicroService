using EcomMicroService.Customer.Localization;
using Volo.Abp.Application.Services;

namespace EcomMicroService.Customer;

public abstract class CustomerAppService : ApplicationService
{
    protected CustomerAppService()
    {
        LocalizationResource = typeof(CustomerResource);
        ObjectMapperContext = typeof(CustomerApplicationModule);
    }
}
