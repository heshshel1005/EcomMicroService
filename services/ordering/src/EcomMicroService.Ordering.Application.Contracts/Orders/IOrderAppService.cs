using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace EcomMicroService.Ordering.Orders;

[Volo.Abp.RemoteService(IsEnabled = false)]
public interface IOrderAppService : IApplicationService
{
    Task<List<OrderDto>> GetMyOrdersAsync();
    Task<OrderDto?> GetAsync(Guid id);
}
