namespace EcomMicroService.Ordering.Orders;

public enum PaymentStatus
{
    None = 0,
    Pending = 1,
    Authorized = 2,
    Paid = 3,
    Failed = 4,
    Refunded = 5,
    CashOnDelivery = 6,
}
