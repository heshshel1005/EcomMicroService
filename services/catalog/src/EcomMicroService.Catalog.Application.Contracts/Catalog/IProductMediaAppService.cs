using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace EcomMicroService.Catalog;

/// <summary>
/// Application service for product media: upload and serve images/videos.
/// </summary>
public interface IProductMediaAppService : IApplicationService
{
    /// <summary>
    /// Upload a product image or video. Requires Catalog permission. Allowed: images (e.g. jpeg, png, webp, gif) and videos (e.g. mp4, webm). Max size 50 MB.
    /// </summary>
    Task<ProductMediaDto> UploadAsync(
        Guid productId,
        IFormFile file,
        ProductMediaType mediaType,
        int sortOrder = 0,
        bool isPrimary = false,
        string? altText = null);

    /// <summary>
    /// Get media metadata by id.
    /// </summary>
    Task<ProductMediaDto?> GetAsync(Guid id);

    /// <summary>
    /// List media for a product, ordered by SortOrder.
    /// </summary>
    Task<List<ProductMediaDto>> GetListByProductIdAsync(Guid productId);

    /// <summary>
    /// Stream the file content for a product media. Used by GET .../product-media/{id}/file. Public (no permission) so storefront can display images.
    /// </summary>
    Task<IRemoteStreamContent?> GetFileAsync(Guid id);

    /// <summary>
    /// Update media metadata (primary, sort order, alt text). Requires Catalog permission.
    /// </summary>
    Task<ProductMediaDto> UpdateAsync(Guid id, UpdateProductMediaDto input);

    /// <summary>
    /// Delete a product media and its stored file. Requires Catalog permission.
    /// </summary>
    Task DeleteAsync(Guid id);
}
