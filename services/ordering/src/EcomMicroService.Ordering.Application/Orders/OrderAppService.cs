using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace EcomMicroService.Ordering.Orders;

[Volo.Abp.RemoteService(IsEnabled = false)]
[Authorize]
public class OrderAppService : ApplicationService, IOrderAppService
{
    private readonly IRepository<Order, Guid> _orderRepository;
    private readonly IRepository<OrderLine, Guid> _orderLineRepository;
    private readonly IRepository<OrderStatusHistory, Guid> _historyRepository;

    public OrderAppService(
        IRepository<Order, Guid> orderRepository,
        IRepository<OrderLine, Guid> orderLineRepository,
        IRepository<OrderStatusHistory, Guid> historyRepository)
    {
        _orderRepository = orderRepository;
        _orderLineRepository = orderLineRepository;
        _historyRepository = historyRepository;
    }

    public async Task<List<OrderDto>> GetMyOrdersAsync()
    {
        var userId = CurrentUser.Id ?? throw new Volo.Abp.Authorization.AbpAuthorizationException("User must be logged in to view orders.");
        var email = CurrentUser.Email;
        var orders = await _orderRepository.GetListAsync(o =>
            o.UserId == userId ||
            (o.UserId == null && email != null && o.ContactEmail != null &&
             o.ContactEmail.ToLower() == email.ToLower()));
        return orders.OrderByDescending(o => o.CreationTime).Select(o => OrderMaps.ToDto(o)).ToList();
    }

    public async Task<OrderDto?> GetAsync(Guid id)
    {
        var userId = CurrentUser.Id;
        var order = await _orderRepository.FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return null;
        if (order.UserId != userId && !await AuthorizationService.IsGrantedAsync("ECommerce.Administration"))
            return null;
        var lines = await _orderLineRepository.GetListAsync(l => l.OrderId == id);
        var history = await _historyRepository.GetListAsync(h => h.OrderId == id);
        return OrderMaps.ToDto(order, lines, history.OrderBy(h => h.CreationTime).ToList());
    }
}
