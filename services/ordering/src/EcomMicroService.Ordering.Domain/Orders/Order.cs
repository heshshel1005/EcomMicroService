using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Ordering.Orders;

public class Order : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid? UserId { get; set; }
    public OrderStatus Status { get; set; }

    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? ContactName { get; set; }

    public string ShippingStreet { get; set; } = string.Empty;
    public string? ShippingStreet2 { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingRegion { get; set; }
    public string? ShippingPostalCode { get; set; }
    public string? ShippingCountry { get; set; }
    public string? ShippingInstructions { get; set; }

    public bool BillingSameAsShipping { get; set; }
    public string? BillingStreet { get; set; }
    public string? BillingStreet2 { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingRegion { get; set; }
    public string? BillingPostalCode { get; set; }
    public string? BillingCountry { get; set; }

    public string? ShippingMethodCode { get; set; }
    public string? ShippingMethodName { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Total { get; set; }

    public PaymentStatus PaymentStatus { get; set; }
    public string? PaymentGateway { get; set; }
    public string? ExternalPaymentId { get; set; }

    public Guid? CouponId { get; set; }
    public decimal DiscountAmount { get; set; }

    public ICollection<OrderLine> Lines { get; private set; } = new List<OrderLine>();

    protected Order()
    {
    }

    public Order(
        Guid id,
        Guid? userId,
        string contactEmail,
        string? contactPhone,
        string? contactName,
        string shippingStreet,
        string? shippingStreet2,
        string? shippingCity,
        string? shippingRegion,
        string? shippingPostalCode,
        string? shippingCountry,
        string? shippingInstructions,
        bool billingSameAsShipping,
        string? shippingMethodCode,
        string? shippingMethodName,
        decimal shippingAmount,
        decimal taxAmount,
        decimal subTotal,
        decimal total,
        Guid? couponId = null,
        decimal discountAmount = 0,
        string? billingStreet = null,
        string? billingStreet2 = null,
        string? billingCity = null,
        string? billingRegion = null,
        string? billingPostalCode = null,
        string? billingCountry = null)
        : base(id)
    {
        UserId = userId;
        Status = OrderStatus.Pending;
        ContactEmail = contactEmail ?? string.Empty;
        ContactPhone = contactPhone;
        ContactName = contactName;
        ShippingStreet = shippingStreet ?? string.Empty;
        ShippingStreet2 = shippingStreet2;
        ShippingCity = shippingCity;
        ShippingRegion = shippingRegion;
        ShippingPostalCode = shippingPostalCode;
        ShippingCountry = shippingCountry;
        ShippingInstructions = shippingInstructions;
        BillingSameAsShipping = billingSameAsShipping;
        BillingStreet = billingStreet;
        BillingStreet2 = billingStreet2;
        BillingCity = billingCity;
        BillingRegion = billingRegion;
        BillingPostalCode = billingPostalCode;
        BillingCountry = billingCountry;
        ShippingMethodCode = shippingMethodCode;
        ShippingMethodName = shippingMethodName;
        ShippingAmount = shippingAmount;
        TaxAmount = taxAmount;
        SubTotal = subTotal;
        Total = total;
        CouponId = couponId;
        DiscountAmount = discountAmount;
    }

    public void AddLine(OrderLine line)
    {
        if (line == null) throw new ArgumentNullException(nameof(line));
        Lines.Add(line);
    }

    public void SetStatus(OrderStatus status) => Status = status;

    public void SetPayment(string gateway, string externalPaymentId, PaymentStatus paymentStatus)
    {
        PaymentGateway = gateway;
        ExternalPaymentId = externalPaymentId;
        PaymentStatus = paymentStatus;
    }
}
