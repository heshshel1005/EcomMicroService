using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Ordering.Orders;

public class OrderLine : AuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductVariantId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;

    protected OrderLine()
    {
    }

    public OrderLine(
        Guid id,
        Guid orderId,
        Guid productVariantId,
        Guid productId,
        string productName,
        string sku,
        decimal unitPrice,
        int quantity)
        : base(id)
    {
        OrderId = orderId;
        ProductVariantId = productVariantId;
        ProductId = productId;
        ProductName = productName ?? string.Empty;
        Sku = sku ?? string.Empty;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }
}
