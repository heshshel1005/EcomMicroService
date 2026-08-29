using System;
using System.Threading.Tasks;
using EcomMicroService.Catalog;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace EcomMicroService.Catalog;

/// <summary>
/// Admin product review API: list, approve, reject, delete.
/// </summary>
[Route("api/catalog/product-review-admin")]
[Area("catalog")]
public class ProductReviewAdminController : CatalogController
{
    private readonly IProductReviewAdminAppService _appService;

    public ProductReviewAdminController(IProductReviewAdminAppService appService)
    {
        _appService = appService;
    }

    [HttpGet]
    public async Task<PagedResultDto<ProductReviewDto>> GetListAsync([FromQuery] ProductReviewListRequestDto input)
    {
        return await _appService.GetListAsync(input);
    }

    [HttpPost("{id}/approve")]
    public async Task ApproveAsync(Guid id)
    {
        await _appService.ApproveAsync(id);
    }

    [HttpPost("{id}/reject")]
    public async Task RejectAsync(Guid id)
    {
        await _appService.RejectAsync(id);
    }

    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await _appService.DeleteAsync(id);
    }
}
