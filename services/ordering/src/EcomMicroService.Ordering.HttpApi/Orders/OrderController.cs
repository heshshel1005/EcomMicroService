using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace EcomMicroService.Ordering.Orders;

[RemoteService(Name = "Ordering")]
[Area("ordering")]
[Route("api/ordering/orders")]
[Authorize]
public class OrderController : OrderingController
{
    private readonly IOrderAppService _appService;

    public OrderController(IOrderAppService appService)
    {
        _appService = appService;
    }

    [HttpGet("my-orders")]
    public Task<List<OrderDto>> GetMyOrdersAsync() => _appService.GetMyOrdersAsync();

    [HttpGet("{id}")]
    public Task<OrderDto?> GetAsync(Guid id) => _appService.GetAsync(id);
}
