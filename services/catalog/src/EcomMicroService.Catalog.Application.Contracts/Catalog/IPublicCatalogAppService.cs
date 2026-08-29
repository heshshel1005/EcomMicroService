using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EcomMicroService.Catalog;

/// <summary>
/// Public (storefront) catalog API: category tree, product list/detail with filters and search.
/// No authentication required. Exposed via PublicCatalogController only (no auto API).
/// </summary>
public interface IPublicCatalogAppService : IApplicationService
{
    /// <summary>
    /// Get the category tree (root nodes with nested children).
    /// </summary>
    Task<List<CategoryTreeDto>> GetCategoryTreeAsync();

    /// <summary>
    /// Get available filter values for storefront in the given category/product-type context.
    /// </summary>
    Task<CatalogFilterOptionsDto> GetFilterOptionsAsync(CatalogFilterOptionsRequestDto input, CancellationToken cancellationToken = default);


    /// <summary>
    /// Get paged product list with optional search and filters (category, price, size, color).
    /// Only published products are returned.
    /// </summary>
    Task<PagedResultDto<PublicProductListDto>> GetProductListAsync(PublicProductListRequestDto input, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a single product by id for storefront. Returns null or 404 if not found or not published.
    /// </summary>
    Task<ProductDto> GetProductDetailAsync(Guid id);

    /// <summary>
    /// Get products by ids for comparison (e.g. compare by product IDs). Only published products; max 4.
    /// </summary>
    Task<List<ProductDto>> GetCompareAsync(List<Guid> productIds);

    Task<List<VariantCartInfoDto>> GetVariantCartInfoAsync(List<Guid> variantIds);
}
