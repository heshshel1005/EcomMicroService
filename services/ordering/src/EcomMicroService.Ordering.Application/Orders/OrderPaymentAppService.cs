using System;
using System.Linq;
using System.Threading.Tasks;
using EcomMicroService.Ordering.Orders;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace EcomMicroService.Ordering;

[Volo.Abp.RemoteService(IsEnabled = false)]
[Authorize]
public class OrderPaymentAppService : ApplicationService, IOrderPaymentAppService
{
    private readonly IRepository<Order, Guid> _orderRepository;
    private readonly IRepository<OrderLine, Guid> _orderLineRepository;
    private readonly IRepository<OrderStatusHistory, Guid> _historyRepository;
    private readonly ShopIntegrationClients _shop;

    public OrderPaymentAppService(
        IRepository<Order, Guid> orderRepository,
        IRepository<OrderLine, Guid> orderLineRepository,
        IRepository<OrderStatusHistory, Guid> historyRepository,
        ShopIntegrationClients shop)
    {
        _orderRepository = orderRepository;
        _orderLineRepository = orderLineRepository;
        _historyRepository = historyRepository;
        _shop = shop;
    }

    public async Task<OrderPaymentSnapshotDto> GetSnapshotAsync(Guid orderId)
    {
        var order = await _orderRepository.GetAsync(orderId);
        await EnsureAccessAsync(order);
        return new OrderPaymentSnapshotDto
        {
            Id = order.Id,
            UserId = order.UserId,
            ContactEmail = order.ContactEmail,
            ContactName = order.ContactName,
            Total = order.Total,
            PaymentStatus = order.PaymentStatus.ToString(),
            PaymentGateway = order.PaymentGateway,
            ExternalPaymentId = order.ExternalPaymentId,
        };
    }

    public async Task ApplyPaymentAsync(Guid orderId, ApplyOrderPaymentDto input)
    {
        var order = await _orderRepository.GetAsync(orderId);
        await EnsureAccessAsync(order);
        if (!Enum.TryParse<PaymentStatus>(input.PaymentStatus, true, out var status))
            status = PaymentStatus.Paid;

        order.SetPayment(input.Gateway, input.ExternalPaymentId, status);
        order.SetStatus(OrderStatus.Confirmed);
        await _orderRepository.UpdateAsync(order);
        await _historyRepository.InsertAsync(new OrderStatusHistory(GuidGenerator.Create(), order.Id, OrderStatus.Confirmed));

        var lines = await _orderLineRepository.GetListAsync(l => l.OrderId == order.Id);
        await _shop.DeductInventoryAsync(lines.Select(l => new ShopIntegrationClients.InventoryLineDto
        {
            ProductVariantId = l.ProductVariantId,
            Quantity = l.Quantity
        }));
        await _shop.AwardLoyaltyAsync(order.Id, order.UserId, order.Total);
    }

    private async Task EnsureAccessAsync(Order order)
    {
        var userId = CurrentUser.Id;
        if (order.UserId == userId) return;
        if (order.UserId == null && CurrentUser.Email != null &&
            order.ContactEmail?.Equals(CurrentUser.Email, StringComparison.OrdinalIgnoreCase) == true) return;
        if (await AuthorizationService.IsGrantedAsync("ECommerce.Administration")) return;
        throw new Volo.Abp.Authorization.AbpAuthorizationException("You do not have access to this order.");
    }
}
