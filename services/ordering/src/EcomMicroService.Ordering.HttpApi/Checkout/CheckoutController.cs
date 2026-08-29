using System;
using System.Threading.Tasks;
using EcomMicroService.Ordering.Checkout;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Volo.Abp;

namespace EcomMicroService.Ordering;

[RemoteService(Name = "Ordering")]
[Area("ordering")]
[Route("api/ordering/checkout")]
public class CheckoutController : OrderingController
{
    private readonly ICheckoutAppService _appService;

    public CheckoutController(ICheckoutAppService appService)
    {
        _appService = appService;
    }

    [AllowAnonymous]
    [HttpGet("summary")]
    public Task<CheckoutSummaryDto> GetSummaryAsync([FromQuery] Guid? guestCartId = null, [FromQuery] string? couponCode = null)
        => _appService.GetSummaryAsync(ResolveGuestCartId(guestCartId), couponCode);

    [AllowAnonymous]
    [HttpPost("submit")]
    public Task<SubmitCheckoutResultDto> SubmitOrderAsync([FromBody] SubmitCheckoutDto input, [FromQuery] Guid? guestCartId = null)
        => _appService.SubmitOrderAsync(input, ResolveGuestCartId(guestCartId));

    private Guid? ResolveGuestCartId(Guid? guestCartId)
    {
        if (guestCartId != null && guestCartId != Guid.Empty)
            return guestCartId;
        if (Request.Headers.TryGetValue("X-Guest-Cart-Id", out var hv) && !StringValues.IsNullOrEmpty(hv) &&
            Guid.TryParse(hv.ToString(), out var g))
            return g;
        return null;
    }
}
