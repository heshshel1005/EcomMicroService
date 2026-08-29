using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace EcomMicroService.Ordering.Orders;

[Volo.Abp.RemoteService(IsEnabled = false)]
[Authorize("ECommerce.Administration")]
public class OrderAdminAppService : ApplicationService, IOrderAdminAppService
{
    private readonly IRepository<Order, Guid> _orderRepository;
    private readonly IRepository<OrderLine, Guid> _orderLineRepository;
    private readonly IRepository<OrderStatusHistory, Guid> _historyRepository;
    private readonly IRepository<Shipment, Guid> _shipmentRepository;
    private readonly ShopIntegrationClients _shop;

    public OrderAdminAppService(
        IRepository<Order, Guid> orderRepository,
        IRepository<OrderLine, Guid> orderLineRepository,
        IRepository<OrderStatusHistory, Guid> historyRepository,
        IRepository<Shipment, Guid> shipmentRepository,
        ShopIntegrationClients shop)
    {
        _orderRepository = orderRepository;
        _orderLineRepository = orderLineRepository;
        _historyRepository = historyRepository;
        _shipmentRepository = shipmentRepository;
        _shop = shop;
    }

    public async Task<PagedResultDto<OrderListDto>> GetListAsync(OrderListRequestDto input)
    {
        var query = await _orderRepository.GetQueryableAsync();
        if (!string.IsNullOrWhiteSpace(input.Status) && Enum.TryParse<OrderStatus>(input.Status, true, out var statusFilter))
            query = query.Where(o => o.Status == statusFilter);
        if (input.DateFrom.HasValue)
            query = query.Where(o => o.CreationTime >= input.DateFrom.Value);
        if (input.DateTo.HasValue)
            query = query.Where(o => o.CreationTime < input.DateTo.Value);
        if (!string.IsNullOrWhiteSpace(input.Search))
        {
            var term = input.Search.Trim().ToLower();
            query = query.Where(o =>
                (o.ContactEmail != null && o.ContactEmail.ToLower().Contains(term)) ||
                (o.ContactName != null && o.ContactName.ToLower().Contains(term)));
        }

        var total = await AsyncExecuter.CountAsync(query);
        var sortDesc = input.Sorting?.EndsWith(" DESC", StringComparison.OrdinalIgnoreCase) ?? true;
        query = sortDesc ? query.OrderByDescending(o => o.CreationTime) : query.OrderBy(o => o.CreationTime);
        var skip = input.SkipCount;
        var take = input.MaxResultCount > 0 ? input.MaxResultCount : 10;
        var orders = await AsyncExecuter.ToListAsync(query.Skip(skip).Take(take));
        var items = orders.Select(o => new OrderListDto
        {
            Id = o.Id,
            Status = o.Status.ToString(),
            PaymentStatus = o.PaymentStatus.ToString(),
            ContactEmail = o.ContactEmail,
            ContactName = o.ContactName,
            Total = o.Total,
            CreationTime = o.CreationTime,
            UserId = o.UserId,
        }).ToList();
        return new PagedResultDto<OrderListDto>(total, items);
    }

    public async Task<OrderDto> GetAsync(Guid id)
    {
        var order = await _orderRepository.GetAsync(id);
        var lines = await _orderLineRepository.GetListAsync(l => l.OrderId == id);
        var history = await _historyRepository.GetListAsync(h => h.OrderId == id);
        return OrderMaps.ToDto(order, lines, history.OrderBy(h => h.CreationTime).ToList());
    }

    public async Task<OrderDto> UpdateStatusAsync(Guid id, UpdateOrderStatusDto input)
    {
        if (!Enum.TryParse<OrderStatus>(input.Status, true, out var newStatus))
            throw new Volo.Abp.BusinessException("ECommerce:InvalidOrderStatus").WithData("Status", input.Status);

        var order = await _orderRepository.GetAsync(id);
        var previousStatus = order.Status;
        if (previousStatus == newStatus)
            return await GetAsync(id);

        if (newStatus == OrderStatus.Confirmed)
        {
            var lines = await _orderLineRepository.GetListAsync(l => l.OrderId == id);
            await _shop.DeductInventoryAsync(lines.Select(l => new ShopIntegrationClients.InventoryLineDto
            {
                ProductVariantId = l.ProductVariantId,
                Quantity = l.Quantity
            }));
            await _shop.AwardLoyaltyAsync(order.Id, order.UserId, order.Total);
        }
        else if (newStatus == OrderStatus.Shipped)
        {
            await _shipmentRepository.InsertAsync(new Shipment(
                GuidGenerator.Create(), order.Id, input.Carrier, input.TrackingNumber, DateTime.UtcNow, null));
        }
        else if (newStatus == OrderStatus.Cancelled && previousStatus >= OrderStatus.Confirmed)
        {
            var lines = await _orderLineRepository.GetListAsync(l => l.OrderId == id);
            await _shop.RestoreInventoryAsync(lines.Select(l => new ShopIntegrationClients.InventoryLineDto
            {
                ProductVariantId = l.ProductVariantId,
                Quantity = l.Quantity
            }));
        }

        order.SetStatus(newStatus);
        await _orderRepository.UpdateAsync(order);
        await _historyRepository.InsertAsync(new OrderStatusHistory(GuidGenerator.Create(), order.Id, newStatus));
        return await GetAsync(id);
    }

    public async Task<List<ShipmentDto>> GetShipmentsAsync(Guid orderId)
    {
        await _orderRepository.GetAsync(orderId);
        var list = await _shipmentRepository.GetListAsync(s => s.OrderId == orderId);
        return list.OrderBy(s => s.CreationTime).Select(MapShipment).ToList();
    }

    public async Task<ShipmentDto> CreateShipmentAsync(Guid orderId, CreateShipmentDto input)
    {
        var order = await _orderRepository.GetAsync(orderId);
        var shipment = new Shipment(GuidGenerator.Create(), orderId, input.Carrier, input.TrackingNumber, DateTime.UtcNow, input.Notes);
        await _shipmentRepository.InsertAsync(shipment);
        if (order.Status != OrderStatus.Shipped && order.Status != OrderStatus.Delivered)
        {
            order.SetStatus(OrderStatus.Shipped);
            await _orderRepository.UpdateAsync(order);
            await _historyRepository.InsertAsync(new OrderStatusHistory(GuidGenerator.Create(), order.Id, OrderStatus.Shipped));
        }
        return MapShipment(shipment);
    }

    public async Task<RefundOrderResultDto> RefundOrderAsync(Guid orderId, decimal? amount = null, string? reason = null)
    {
        var order = await _orderRepository.GetAsync(orderId);
        var result = await _shop.RefundViaPaymentAsync(orderId, amount, reason);
        if (result.Success && order.Status >= OrderStatus.Confirmed)
        {
            order.SetStatus(OrderStatus.Cancelled);
            await _orderRepository.UpdateAsync(order);
            await _historyRepository.InsertAsync(new OrderStatusHistory(GuidGenerator.Create(), order.Id, OrderStatus.Cancelled));
            var lines = await _orderLineRepository.GetListAsync(l => l.OrderId == orderId);
            await _shop.RestoreInventoryAsync(lines.Select(l => new ShopIntegrationClients.InventoryLineDto
            {
                ProductVariantId = l.ProductVariantId,
                Quantity = l.Quantity
            }));
        }
        return new RefundOrderResultDto
        {
            Success = result.Success,
            ErrorCode = result.ErrorCode,
            ErrorMessage = result.ErrorMessage,
        };
    }

    private static ShipmentDto MapShipment(Shipment s) => new()
    {
        Id = s.Id,
        OrderId = s.OrderId,
        Carrier = s.Carrier,
        TrackingNumber = s.TrackingNumber,
        ShippedAt = s.ShippedAt,
        Notes = s.Notes,
        CreationTime = s.CreationTime,
    };
}
