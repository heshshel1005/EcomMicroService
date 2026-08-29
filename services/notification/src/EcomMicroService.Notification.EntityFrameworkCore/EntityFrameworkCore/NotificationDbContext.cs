using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace EcomMicroService.Notification.EntityFrameworkCore;

[ConnectionStringName(EcomMicroServiceNames.NotificationDb)]
public class NotificationDbContext(DbContextOptions<NotificationDbContext> options)
    : AbpDbContext<NotificationDbContext>(options),
        INotificationDbContext
{
    public DbSet<UserNotification> UserNotifications { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ConfigureNotification();
    }
}
