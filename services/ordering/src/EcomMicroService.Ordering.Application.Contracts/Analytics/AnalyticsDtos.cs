using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace EcomMicroService.Ordering.Analytics;

[Volo.Abp.RemoteService(IsEnabled = false)]
public interface IAnalyticsAppService : IApplicationService
{
    Task<SalesSummaryDto> GetSalesSummaryAsync(AnalyticsFilterDto input);
    Task<List<SalesByDayDto>> GetSalesByDayAsync(AnalyticsFilterDto input);
    Task<List<TopProductDto>> GetTopProductsAsync(AnalyticsFilterDto input, int maxCount = 10);
    Task<string> ExportSalesCsvAsync(AnalyticsFilterDto input);
}

public class AnalyticsFilterDto
{
    public System.DateTime? DateFrom { get; set; }
    public System.DateTime? DateTo { get; set; }
}

public class SalesSummaryDto
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public System.DateTime? PeriodStart { get; set; }
    public System.DateTime? PeriodEnd { get; set; }
}

public class SalesByDayDto
{
    public System.DateTime Date { get; set; }
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
}

public class TopProductDto
{
    public System.Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Revenue { get; set; }
}
