using System.Collections.Generic;
using System.Linq;
using EcomMicroService.Ordering.Orders;

namespace EcomMicroService.Ordering;

internal static class OrderMaps
{
    public static OrderDto ToDto(Order order, List<OrderLine>? lines = null, List<OrderStatusHistory>? statusHistory = null)
    {
        var dto = new OrderDto
        {
            Id = order.Id,
            Status = order.Status.ToString(),
            ContactEmail = order.ContactEmail,
            ContactPhone = order.ContactPhone,
            ContactName = order.ContactName,
            ShippingStreet = order.ShippingStreet,
            ShippingStreet2 = order.ShippingStreet2,
            ShippingCity = order.ShippingCity,
            ShippingRegion = order.ShippingRegion,
            ShippingPostalCode = order.ShippingPostalCode,
            ShippingCountry = order.ShippingCountry,
            ShippingMethodName = order.ShippingMethodName,
            SubTotal = order.SubTotal,
            ShippingAmount = order.ShippingAmount,
            TaxAmount = order.TaxAmount,
            Total = order.Total,
            PaymentStatus = order.PaymentStatus.ToString(),
            PaymentGateway = order.PaymentGateway,
            ExternalPaymentId = order.ExternalPaymentId,
            CreationTime = order.CreationTime,
        };
        var lineList = lines ?? (order.Lines?.ToList() ?? new List<OrderLine>());
        dto.Lines = lineList.Select(l => new OrderLineDto
        {
            Id = l.Id,
            ProductVariantId = l.ProductVariantId,
            ProductId = l.ProductId,
            ProductName = l.ProductName,
            Sku = l.Sku,
            UnitPrice = l.UnitPrice,
            Quantity = l.Quantity,
            LineTotal = l.LineTotal,
        }).ToList();
        if (statusHistory != null)
        {
            dto.StatusHistory = statusHistory.Select(h => new OrderStatusHistoryDto
            {
                Id = h.Id,
                OrderId = h.OrderId,
                Status = h.Status.ToString(),
                CreationTime = h.CreationTime,
            }).ToList();
        }
        return dto;
    }
}
