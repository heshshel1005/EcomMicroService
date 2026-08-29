namespace EcomMicroService.Catalog;

public static class CatalogDbProperties
{
    public const string ConnectionStringName = "Catalog";
    public static string DbTablePrefix { get; set; } = "Catalog";

    public static string DbSchema { get; set; } = null;
}
