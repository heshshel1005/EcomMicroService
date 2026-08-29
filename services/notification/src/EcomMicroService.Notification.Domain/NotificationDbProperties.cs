namespace EcomMicroService.Notification;

public static class NotificationDbProperties
{
    public const string ConnectionStringName = "Notification";
    public static string DbTablePrefix { get; set; } = "Notification";
    public static string DbSchema { get; set; } = null;
}
