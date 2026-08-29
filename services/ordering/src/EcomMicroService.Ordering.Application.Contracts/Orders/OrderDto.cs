using System;
using System.Collections.Generic;

namespace EcomMicroService.Ordering.Orders;

public class OrderDto
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? ContactName { get; set; }
    public string ShippingStreet { get; set; } = string.Empty;
    public string? ShippingStreet2 { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingRegion { get; set; }
    public string? ShippingPostalCode { get; set; }
    public string? ShippingCountry { get; set; }
    public string? ShippingMethodName { get; set; }
    public decimal SubTotal { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public string PaymentStatus { get; set; } = "None";
    public string? PaymentGateway { get; set; }
    public string? ExternalPaymentId { get; set; }
    public DateTime CreationTime { get; set; }
    public List<OrderLineDto> Lines { get; set; } = new();
    public List<OrderStatusHistoryDto> StatusHistory { get; set; } = new();
}
