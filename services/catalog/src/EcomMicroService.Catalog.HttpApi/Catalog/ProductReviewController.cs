using System;
using System.Threading.Tasks;
using EcomMicroService.Catalog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace EcomMicroService.Catalog;

/// <summary>
/// Public product review API: aggregate and list for PDP; submit (authenticated).
/// </summary>
[Route("api/catalog/product-review")]
[Area("catalog")]
public class ProductReviewController : CatalogController
{
    private readonly IProductReviewAppService _appService;

    public ProductReviewController(IProductReviewAppService appService)
    {
        _appService = appService;
    }

    [AllowAnonymous]
    [HttpGet("products/{productId}/aggregate")]
    public async Task<ProductReviewAggregateDto> GetAggregateAsync(Guid productId)
    {
        return await _appService.GetAggregateAsync(productId);
    }

    [AllowAnonymous]
    [HttpGet("products/{productId}/reviews")]
    public async Task<PagedResultDto<ProductReviewDto>> GetListAsync(Guid productId, [FromQuery] PagedAndSortedResultRequestDto input)
    {
        return await _appService.GetListAsync(productId, input);
    }

    [Authorize]
    [HttpPost]
    public async Task<ProductReviewDto> SubmitAsync([FromBody] CreateProductReviewDto input)
    {
        return await _appService.SubmitAsync(input);
    }
}
