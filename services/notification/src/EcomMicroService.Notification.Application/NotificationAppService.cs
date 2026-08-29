using EcomMicroService.Notification.Localization;
using Volo.Abp.Application.Services;

namespace EcomMicroService.Notification;

public abstract class NotificationAppService : ApplicationService
{
    protected NotificationAppService()
    {
        LocalizationResource = typeof(NotificationResource);
        ObjectMapperContext = typeof(NotificationApplicationModule);
    }
}
