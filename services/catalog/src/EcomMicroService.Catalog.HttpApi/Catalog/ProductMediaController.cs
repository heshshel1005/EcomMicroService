using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EcomMicroService.Catalog;
using EcomMicroService.Catalog.Localization;
using EcomMicroService.Catalog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;

namespace EcomMicroService.Catalog;

[Route("api/catalog/product-media")]
[Area("catalog")]
public class ProductMediaController : CatalogController
{
    private readonly IProductMediaAppService _appService;

    public ProductMediaController(IProductMediaAppService appService)
    {
        _appService = appService;
    }

    /// <summary>
    /// Upload a product image or video. Requires Catalog permission. Send as multipart/form-data.
    /// </summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ProductMediaDto), StatusCodes.Status200OK)]
    public async Task<ProductMediaDto> UploadAsync([FromForm] ProductMediaUploadRequest request)
    {
        if (request.File == null || request.File.Length == 0)
            throw new UserFriendlyException(L["ProductMediaFileRequired"]);

        return await _appService.UploadAsync(
            request.ProductId,
            request.File,
            request.MediaType,
            request.SortOrder,
            request.IsPrimary,
            request.AltText);
    }

    /// <summary>
    /// Get product media metadata by id.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ProductMediaDto?> GetAsync(Guid id)
    {
        return await _appService.GetAsync(id);
    }

    /// <summary>
    /// List media for a product.
    /// </summary>
    [HttpGet("by-product/{productId}")]
    public async Task<List<ProductMediaDto>> GetListByProductIdAsync(Guid productId)
    {
        return await _appService.GetListByProductIdAsync(productId);
    }

    /// <summary>
    /// Stream the file content (image or video). Public endpoint for storefront display.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id}/file")]
    public async Task<IActionResult> GetFileAsync(Guid id)
    {
        var content = await _appService.GetFileAsync(id);
        if (content == null)
            return NotFound();

        return new FileStreamResult(content.GetStream(), content.ContentType)
        {
            FileDownloadName = content.FileName,
            EnableRangeProcessing = true
        };
    }

    /// <summary>
    /// Update product media metadata (primary, sort order, alt text). Requires Catalog permission.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ProductMediaDto> UpdateAsync(Guid id, [FromBody] UpdateProductMediaDto input)
    {
        return await _appService.UpdateAsync(id, input);
    }

    /// <summary>
    /// Delete a product media and its file. Requires Catalog permission.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await _appService.DeleteAsync(id);
    }
}
