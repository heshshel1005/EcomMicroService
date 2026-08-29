namespace EcomMicroService.Ordering;

public static class OrderingDbProperties
{
    public const string ConnectionStringName = "Ordering";
    public static string DbTablePrefix { get; set; } = "Ordering";

    public static string DbSchema { get; set; } = null;
}
