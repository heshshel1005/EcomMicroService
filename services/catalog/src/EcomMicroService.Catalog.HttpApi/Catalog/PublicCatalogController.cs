using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EcomMicroService.Catalog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace EcomMicroService.Catalog;

/// <summary>
/// Public (storefront) catalog API: category tree, product list/detail with filters and search.
/// No authentication required.
/// </summary>
[AllowAnonymous]
[Route("api/catalog/public-catalog")]
[Area("catalog")]
public class PublicCatalogController : CatalogController
{
    private readonly IPublicCatalogAppService _appService;

    public PublicCatalogController(IPublicCatalogAppService appService)
    {
        _appService = appService;
    }

    /// <summary>
    /// Get the category tree (root nodes with nested children).
    /// </summary>
    [HttpGet("categories/tree")]
    public async Task<List<CategoryTreeDto>> GetCategoryTreeAsync()
    {
        return await _appService.GetCategoryTreeAsync();
    }

    /// <summary>
    /// Get available filter values for storefront.
    /// </summary>
    [HttpGet("filter-options")]
    public async Task<CatalogFilterOptionsDto> GetFilterOptionsAsync([FromQuery] CatalogFilterOptionsRequestDto input, CancellationToken cancellationToken)
    {
        return await _appService.GetFilterOptionsAsync(input, cancellationToken);
    }

    /// <summary>
    /// Get paged product list with optional search and filters (category, price, size, color).
    /// </summary>
    [HttpGet("products")]
    [HttpGet("product-list")] // alternate route for compatibility
    public async Task<PagedResultDto<PublicProductListDto>> GetProductListAsync([FromQuery] PublicProductListRequestDto input, CancellationToken cancellationToken)
    {
        return await _appService.GetProductListAsync(input, cancellationToken);
    }

    /// <summary>
    /// Get a single product by id for storefront (published only).
    /// </summary>
    [HttpGet("products/{id}")]
    public async Task<ProductDto> GetProductDetailAsync(Guid id)
    {
        return await _appService.GetProductDetailAsync(id);
    }

    /// <summary>
    /// Get products by ids for comparison (published only; max 4).
    /// Pass productIds=guid1&amp;productIds=guid2 or ids=guid1,guid2.
    /// </summary>
    [HttpGet("compare")]
    public async Task<List<ProductDto>> GetCompareAsync(
        [FromQuery] List<Guid>? productIds = null,
        [FromQuery] string? ids = null)
    {
        var list = productIds ?? new List<Guid>();
        if (list.Count == 0 && !string.IsNullOrWhiteSpace(ids))
        {
            foreach (var part in ids!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (Guid.TryParse(part, out var g))
                    list.Add(g);
            }
        }
        return await _appService.GetCompareAsync(list);
    }

    [HttpGet("variants/cart-info")]
    public Task<List<VariantCartInfoDto>> GetVariantCartInfoAsync([FromQuery] string? ids)
    {
        var list = new List<Guid>();
        if (!string.IsNullOrWhiteSpace(ids))
        {
            foreach (var part in ids.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (Guid.TryParse(part, out var g))
                {
                    list.Add(g);
                }
            }
        }

        return _appService.GetVariantCartInfoAsync(list);
    }
}
