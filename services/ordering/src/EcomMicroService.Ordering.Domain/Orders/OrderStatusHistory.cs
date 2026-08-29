using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Ordering.Orders;

public class OrderStatusHistory : CreationAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid OrderId { get; set; }
    public OrderStatus Status { get; set; }

    protected OrderStatusHistory()
    {
    }

    public OrderStatusHistory(Guid id, Guid orderId, OrderStatus status)
        : base(id)
    {
        OrderId = orderId;
        Status = status;
    }
}
