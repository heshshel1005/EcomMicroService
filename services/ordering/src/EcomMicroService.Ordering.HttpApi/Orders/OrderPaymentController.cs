using System;
using System.Threading.Tasks;
using EcomMicroService.Ordering.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace EcomMicroService.Ordering;

[RemoteService(Name = "Ordering")]
[Area("ordering")]
[Route("api/ordering/orders")]
[Authorize]
public class OrderPaymentController : OrderingController
{
    private readonly IOrderPaymentAppService _appService;

    public OrderPaymentController(IOrderPaymentAppService appService)
    {
        _appService = appService;
    }

    [HttpGet("{id}/payment-snapshot")]
    public Task<OrderPaymentSnapshotDto> GetSnapshotAsync(Guid id) => _appService.GetSnapshotAsync(id);

    [HttpPost("{id}/apply-payment")]
    public Task ApplyPaymentAsync(Guid id, [FromBody] ApplyOrderPaymentDto input) => _appService.ApplyPaymentAsync(id, input);
}
