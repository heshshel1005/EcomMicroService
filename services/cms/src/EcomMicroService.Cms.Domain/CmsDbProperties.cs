namespace EcomMicroService.Cms;

public static class CmsDbProperties
{
    public const string ConnectionStringName = "Cms";
    public static string DbTablePrefix { get; set; } = "Cms";
    public static string DbSchema { get; set; } = null;
}
