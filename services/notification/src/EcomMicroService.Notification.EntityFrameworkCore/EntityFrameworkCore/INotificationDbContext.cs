using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace EcomMicroService.Notification.EntityFrameworkCore;

[ConnectionStringName(EcomMicroServiceNames.NotificationDb)]
public interface INotificationDbContext : IEfCoreDbContext
{
}
