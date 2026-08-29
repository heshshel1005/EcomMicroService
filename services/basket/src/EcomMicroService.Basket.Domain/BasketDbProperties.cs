namespace EcomMicroService.Basket;

public static class BasketDbProperties
{
    public const string ConnectionStringName = "Basket";
    public static string DbTablePrefix { get; set; } = "Basket";

    public static string DbSchema { get; set; } = null;
}
