namespace EcomMicroService.Payment;

public static class PaymentDbProperties
{
    public const string ConnectionStringName = "Payment";
    public static string DbTablePrefix { get; set; } = "Payment";
    public static string DbSchema { get; set; } = null;
}
