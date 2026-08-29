using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace EcomMicroService.Notification.EntityFrameworkCore;

public static class NotificationDbContextModelCreatingExtensions
{
    public static void ConfigureNotification(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));
        builder.Entity<UserNotification>(b =>
        {
            b.ToTable(NotificationDbProperties.DbTablePrefix + "UserNotifications", NotificationDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Title).IsRequired().HasMaxLength(256);
            b.HasIndex(x => x.UserId);
        });
    }
}
