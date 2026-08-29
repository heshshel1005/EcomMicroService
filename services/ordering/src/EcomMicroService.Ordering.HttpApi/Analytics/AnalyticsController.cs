using System.Collections.Generic;
using System.Threading.Tasks;
using EcomMicroService.Ordering.Analytics;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace EcomMicroService.Ordering;

[RemoteService(Name = "Ordering")]
[Area("ordering")]
[Route("api/ordering/analytics")]
public class AnalyticsController : OrderingController
{
    private readonly IAnalyticsAppService _appService;

    public AnalyticsController(IAnalyticsAppService appService)
    {
        _appService = appService;
    }

    [HttpGet("summary")]
    public Task<SalesSummaryDto> GetSalesSummaryAsync([FromQuery] AnalyticsFilterDto input) => _appService.GetSalesSummaryAsync(input);

    [HttpGet("by-day")]
    public Task<List<SalesByDayDto>> GetSalesByDayAsync([FromQuery] AnalyticsFilterDto input) => _appService.GetSalesByDayAsync(input);

    [HttpGet("top-products")]
    public Task<List<TopProductDto>> GetTopProductsAsync([FromQuery] AnalyticsFilterDto input, [FromQuery] int maxCount = 10) =>
        _appService.GetTopProductsAsync(input, maxCount);

    [HttpGet("export")]
    public Task<string> ExportSalesCsvAsync([FromQuery] AnalyticsFilterDto input) => _appService.ExportSalesCsvAsync(input);
}
