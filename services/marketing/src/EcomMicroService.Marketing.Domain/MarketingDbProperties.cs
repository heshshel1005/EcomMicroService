namespace EcomMicroService.Marketing;

public static class MarketingDbProperties
{
    public const string ConnectionStringName = "Marketing";
    public static string DbTablePrefix { get; set; } = "Marketing";
    public static string DbSchema { get; set; } = null;
}
