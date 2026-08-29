namespace EcomMicroService.Customer;

public static class CustomerDbProperties
{
    public const string ConnectionStringName = "Customer";
    public static string DbTablePrefix { get; set; } = "Customer";
    public static string DbSchema { get; set; } = null;
}
