using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace EcomMicroService.Catalog;

public class InventoryLineRequestDto
{
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; }
}

[RemoteService(Name = CatalogRemoteServiceConsts.RemoteServiceName)]
[Area("catalog")]
[Route("api/catalog/inventory-deduction")]
[AllowAnonymous]
public class InventoryDeductionController : CatalogController
{
    private readonly IInventoryDeductionService _deduction;
    private readonly IInventoryReservationService _reservation;

    public InventoryDeductionController(
        IInventoryDeductionService deduction,
        IInventoryReservationService reservation)
    {
        _deduction = deduction;
        _reservation = reservation;
    }

    [HttpPost("deduct")]
    public Task DeductAsync([FromBody] List<InventoryLineRequestDto> lines) =>
        _deduction.DeductForOrderLinesAsync(Map(lines));

    [HttpPost("restore")]
    public Task RestoreAsync([FromBody] List<InventoryLineRequestDto> lines) =>
        _deduction.RestoreForOrderLinesAsync(Map(lines));

    [HttpPost("release-reservations")]
    public Task ReleaseReservationsAsync([FromBody] List<InventoryLineRequestDto> lines) =>
        _reservation.ReleaseForCartItemsAsync(Map(lines));

    private static IEnumerable<(Guid ProductVariantId, int Quantity)> Map(List<InventoryLineRequestDto>? lines) =>
        (lines ?? []).Select(l => (l.ProductVariantId, l.Quantity));
}
