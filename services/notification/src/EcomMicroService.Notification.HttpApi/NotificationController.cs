using EcomMicroService.Notification.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace EcomMicroService.Notification;

public abstract class NotificationController : AbpControllerBase
{
    protected NotificationController()
    {
        LocalizationResource = typeof(NotificationResource);
    }
}
