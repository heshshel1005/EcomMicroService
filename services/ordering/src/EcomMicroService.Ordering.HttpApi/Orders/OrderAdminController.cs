using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

namespace EcomMicroService.Ordering.Orders;

[RemoteService(Name = "Ordering")]
[Area("ordering")]
[Route("api/ordering/order-admin")]
public class OrderAdminController : OrderingController
{
    private readonly IOrderAdminAppService _appService;

    public OrderAdminController(IOrderAdminAppService appService)
    {
        _appService = appService;
    }

    [HttpGet]
    public Task<PagedResultDto<OrderListDto>> GetListAsync([FromQuery] OrderListRequestDto input) => _appService.GetListAsync(input);

    [HttpGet("{id}")]
    public Task<OrderDto> GetAsync(Guid id) => _appService.GetAsync(id);

    [HttpPut("{id}/status")]
    public Task<OrderDto> UpdateStatusAsync(Guid id, [FromBody] UpdateOrderStatusDto input) => _appService.UpdateStatusAsync(id, input);

    [HttpGet("{orderId}/shipments")]
    public Task<List<ShipmentDto>> GetShipmentsAsync(Guid orderId) => _appService.GetShipmentsAsync(orderId);

    [HttpPost("{orderId}/shipments")]
    public Task<ShipmentDto> CreateShipmentAsync(Guid orderId, [FromBody] CreateShipmentDto input) =>
        _appService.CreateShipmentAsync(orderId, input);

    [HttpPost("{orderId}/refund")]
    public Task<RefundOrderResultDto> RefundOrderAsync(Guid orderId, [FromQuery] decimal? amount = null, [FromQuery] string? reason = null) =>
        _appService.RefundOrderAsync(orderId, amount, reason);
}
