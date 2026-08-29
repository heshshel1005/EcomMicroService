using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EcomMicroService.Catalog.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;

namespace EcomMicroService.Catalog;

public class ProductMediaAppService : CatalogAppService, IProductMediaAppService
{
    private const int MaxFileSizeBytes = 50 * 1024 * 1024; // 50 MB
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private static readonly HashSet<string> AllowedVideoExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".mp4", ".webm" };

    private readonly IRepository<ProductMedia, Guid> _mediaRepository;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IProductMediaFileStorage _fileStorage;

    public ProductMediaAppService(
        IRepository<ProductMedia, Guid> mediaRepository,
        IRepository<Product, Guid> productRepository,
        IProductMediaFileStorage fileStorage)
    {
        _mediaRepository = mediaRepository;
        _productRepository = productRepository;
        _fileStorage = fileStorage;
    }

    [Authorize(CatalogPermissions.Catalog.Default)]
    public async Task<ProductMediaDto> UploadAsync(
        Guid productId,
        IFormFile file,
        ProductMediaType mediaType,
        int sortOrder = 0,
        bool isPrimary = false,
        string? altText = null)
    {
        var product = await _productRepository.GetAsync(productId);
        if (product == null)
            throw new Volo.Abp.BusinessException("ECommerce:ProductNotFound").WithData("ProductId", productId);

        var fileName = file.FileName ?? "file";
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(extension))
            throw new Volo.Abp.BusinessException("ECommerce:ProductMediaInvalidFile").WithData("FileName", fileName);

        var allowed = mediaType == ProductMediaType.Image ? AllowedImageExtensions : AllowedVideoExtensions;
        if (!allowed.Contains(extension))
            throw new Volo.Abp.BusinessException("ECommerce:ProductMediaInvalidFileType")
                .WithData("MediaType", mediaType.ToString()).WithData("FileName", fileName);

        if (file.Length > MaxFileSizeBytes)
            throw new Volo.Abp.BusinessException("ECommerce:ProductMediaFileTooLarge")
                .WithData("MaxSizeMb", MaxFileSizeBytes / (1024 * 1024));

        await using var stream = file.OpenReadStream();
        var relativePath = await _fileStorage.SaveAsync(stream, fileName, productId);

        var id = GuidGenerator.Create();
        var media = new ProductMedia(id, productId, mediaType, relativePath, sortOrder, isPrimary, altText);
        await _mediaRepository.InsertAsync(media);

        if (isPrimary)
            await ClearPrimaryForProductExceptAsync(productId, id);

        return MapToDto(media);
    }

    public async Task<ProductMediaDto?> GetAsync(Guid id)
    {
        var media = await _mediaRepository.FindAsync(id);
        return media == null ? null : MapToDto(media);
    }

    public async Task<List<ProductMediaDto>> GetListByProductIdAsync(Guid productId)
    {
        var list = await _mediaRepository.GetListAsync(x => x.ProductId == productId);
        return list.OrderBy(x => x.SortOrder).Select(MapToDto).ToList();
    }

    public async Task<IRemoteStreamContent?> GetFileAsync(Guid id)
    {
        var media = await _mediaRepository.FindAsync(id);
        if (media == null)
            return null;

        var opened = await _fileStorage.OpenReadAsync(media.FilePathOrBlobKey);
        if (opened == null)
            return null;

        return new RemoteStreamContent(opened.Value.Stream, opened.Value.FileName, opened.Value.ContentType);
    }

    [Authorize(CatalogPermissions.Catalog.Default)]
    public async Task<ProductMediaDto> UpdateAsync(Guid id, UpdateProductMediaDto input)
    {
        var media = await _mediaRepository.GetAsync(id);
        media.IsPrimary = input.IsPrimary;
        media.SortOrder = input.SortOrder;
        media.AltText = input.AltText;
        await _mediaRepository.UpdateAsync(media);
        if (input.IsPrimary)
            await ClearPrimaryForProductExceptAsync(media.ProductId, id);
        return MapToDto(media);
    }

    [Authorize(CatalogPermissions.Catalog.Default)]
    public async Task DeleteAsync(Guid id)
    {
        var media = await _mediaRepository.GetAsync(id);
        await _fileStorage.DeleteAsync(media.FilePathOrBlobKey);
        await _mediaRepository.DeleteAsync(media);
    }

    private async Task ClearPrimaryForProductExceptAsync(Guid productId, Guid exceptId)
    {
        var list = await _mediaRepository.GetListAsync(x => x.ProductId == productId && x.Id != exceptId && x.IsPrimary);
        foreach (var m in list)
        {
            // Entity is not designed with ClearPrimary method; we'd need to update. For simplicity we could add a property setter.
            // ProductMedia has IsPrimary as get/set, so we can load, set false, update. But ABP's AuditedEntity might not track this.
            // Simplest: run raw update or load each and set IsPrimary = false then UpdateAsync. Let me check ProductMedia - it has public setters.
            m.IsPrimary = false;
        }
        foreach (var m in list)
            await _mediaRepository.UpdateAsync(m);
    }

    private static ProductMediaDto MapToDto(ProductMedia media)
    {
        return new ProductMediaDto
        {
            Id = media.Id,
            ProductId = media.ProductId,
            MediaType = media.MediaType,
            SortOrder = media.SortOrder,
            IsPrimary = media.IsPrimary,
            AltText = media.AltText,
            CreationTime = media.CreationTime
        };
    }
}
