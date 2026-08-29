using System;
using Volo.Abp.Application.Dtos;

namespace EcomMicroService.Ordering.Orders;

public class OrderListRequestDto : PagedAndSortedResultRequestDto
{
    public string? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Search { get; set; }
}

public class OrderListDto
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public decimal Total { get; set; }
    public DateTime CreationTime { get; set; }
    public Guid? UserId { get; set; }
}

public class UpdateOrderStatusDto
{
    public string Status { get; set; } = string.Empty;
    public string? TrackingNumber { get; set; }
    public string? Carrier { get; set; }
}

public class CreateShipmentDto
{
    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }
    public string? Notes { get; set; }
}

public class ShipmentDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }
    public DateTime? ShippedAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreationTime { get; set; }
}

public class OrderStatusHistoryDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreationTime { get; set; }
}

public class RefundOrderResultDto
{
    public bool Success { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

public class OrderPaymentSnapshotDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public decimal Total { get; set; }
    public string PaymentStatus { get; set; } = "None";
    public string? PaymentGateway { get; set; }
    public string? ExternalPaymentId { get; set; }
}

public class ApplyOrderPaymentDto
{
    public string Gateway { get; set; } = string.Empty;
    public string ExternalPaymentId { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = "Paid";
}

public interface IOrderPaymentAppService : Volo.Abp.Application.Services.IApplicationService
{
    System.Threading.Tasks.Task<OrderPaymentSnapshotDto> GetSnapshotAsync(System.Guid orderId);
    System.Threading.Tasks.Task ApplyPaymentAsync(System.Guid orderId, ApplyOrderPaymentDto input);
}
