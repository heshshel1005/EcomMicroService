using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EcomMicroService.Ordering.Analytics;
using EcomMicroService.Ordering.Orders;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace EcomMicroService.Ordering;

[Volo.Abp.RemoteService(IsEnabled = false)]
[Authorize("ECommerce.Analytics")]
public class AnalyticsAppService : ApplicationService, IAnalyticsAppService
{
    private readonly IRepository<Order, Guid> _orderRepository;
    private readonly IRepository<OrderLine, Guid> _orderLineRepository;

    public AnalyticsAppService(
        IRepository<Order, Guid> orderRepository,
        IRepository<OrderLine, Guid> orderLineRepository)
    {
        _orderRepository = orderRepository;
        _orderLineRepository = orderLineRepository;
    }

    public async Task<SalesSummaryDto> GetSalesSummaryAsync(AnalyticsFilterDto input)
    {
        var query = await BuildFilteredOrderQueryAsync(input);
        var totalOrders = await AsyncExecuter.CountAsync(query);
        var totalRevenue = totalOrders == 0 ? 0m : await AsyncExecuter.SumAsync(query.Select(o => o.Total));
        return new SalesSummaryDto
        {
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,
            PeriodStart = input.DateFrom,
            PeriodEnd = input.DateTo
        };
    }

    public async Task<List<SalesByDayDto>> GetSalesByDayAsync(AnalyticsFilterDto input)
    {
        var query = await BuildFilteredOrderQueryAsync(input);
        var grouped = query
            .GroupBy(o => o.CreationTime.Date)
            .Select(g => new SalesByDayDto
            {
                Date = g.Key,
                OrderCount = g.Count(),
                Revenue = g.Sum(o => o.Total)
            })
            .OrderBy(x => x.Date);
        return await AsyncExecuter.ToListAsync(grouped);
    }

    public async Task<List<TopProductDto>> GetTopProductsAsync(AnalyticsFilterDto input, int maxCount = 10)
    {
        var orderQuery = await BuildFilteredOrderQueryAsync(input);
        var orderIdsQuery = orderQuery.Select(o => o.Id);
        var linesQuery = await _orderLineRepository.GetQueryableAsync();
        linesQuery = linesQuery.Where(l => orderIdsQuery.Contains(l.OrderId));
        var grouped = linesQuery
            .GroupBy(l => new { l.ProductId, l.ProductName })
            .Select(g => new TopProductDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.ProductName,
                Quantity = g.Sum(l => l.Quantity),
                Revenue = g.Sum(l => l.UnitPrice * l.Quantity)
            })
            .OrderByDescending(x => x.Quantity)
            .Take(maxCount);
        return await AsyncExecuter.ToListAsync(grouped);
    }

    public async Task<string> ExportSalesCsvAsync(AnalyticsFilterDto input)
    {
        var byDay = await GetSalesByDayAsync(input);
        var sb = new StringBuilder();
        sb.AppendLine("Date,OrderCount,Revenue");
        foreach (var row in byDay)
            sb.AppendLine($"{row.Date:yyyy-MM-dd},{row.OrderCount},{row.Revenue}");
        return sb.ToString();
    }

    private async Task<IQueryable<Order>> BuildFilteredOrderQueryAsync(AnalyticsFilterDto input)
    {
        var query = await _orderRepository.GetQueryableAsync();
        query = query.Where(o => o.Status != OrderStatus.Cancelled);
        if (input.DateFrom.HasValue)
            query = query.Where(o => o.CreationTime >= input.DateFrom.Value);
        if (input.DateTo.HasValue)
            query = query.Where(o => o.CreationTime < input.DateTo.Value);
        return query;
    }
}
