using EcomMicroService.Customer.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace EcomMicroService.Customer;

public abstract class CustomerController : AbpControllerBase
{
    protected CustomerController()
    {
        LocalizationResource = typeof(CustomerResource);
    }
}
