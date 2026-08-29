using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace EcomMicroService.Catalog;

public class ProductMediaFileStorage : IProductMediaFileStorage
{
    private readonly IWebHostEnvironment _env;
    private static readonly Dictionary<string, string> ContentTypesByExtension = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".png", "image/png" },
        { ".gif", "image/gif" },
        { ".webp", "image/webp" },
        { ".mp4", "video/mp4" },
        { ".webm", "video/webm" }
    };

    public ProductMediaFileStorage(IWebHostEnvironment env)
    {
        _env = env;
    }

    private string GetRootPath()
    {
        var path = Path.Combine(_env.ContentRootPath, "App_Data", "ProductMedia");
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        return path;
    }

    public async Task<string> SaveAsync(Stream stream, string fileName, Guid productId, CancellationToken cancellationToken = default)
    {
        var root = GetRootPath();
        var productDir = Path.Combine(root, productId.ToString("N"));
        if (!Directory.Exists(productDir))
            Directory.CreateDirectory(productDir);

        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(ext))
            ext = ".bin";
        var safeName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(productDir, safeName);
        var relativePath = $"App_Data/ProductMedia/{productId:N}/{safeName}";

        await using (var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
        {
            await stream.CopyToAsync(fs, cancellationToken);
        }

        return relativePath;
    }

    public Task<(Stream Stream, string ContentType, string FileName)?> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_env.ContentRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullPath))
            return Task.FromResult<(Stream, string, string)?>(null);

        var ext = Path.GetExtension(fullPath);
        var contentType = ContentTypesByExtension.TryGetValue(ext, out var ct) ? ct : "application/octet-stream";
        var fileName = Path.GetFileName(fullPath);
        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);

        return Task.FromResult<(Stream, string, string)?>((stream, contentType, fileName));
    }

    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_env.ContentRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }
}
