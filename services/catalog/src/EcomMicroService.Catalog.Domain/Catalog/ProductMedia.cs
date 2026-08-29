using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Catalog;

/// <summary>
/// Product image or video. Stored in DB (bytea/path) or backend server; primary image used in listings and PDP.
/// </summary>
public class ProductMedia : AuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid ProductId { get; set; }
    public ProductMediaType MediaType { get; set; }
    /// <summary>File path on server or blob key for stored content.</summary>
    public string FilePathOrBlobKey { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }
    public string? AltText { get; set; }

    protected ProductMedia()
    {
    }

    public ProductMedia(
        Guid id,
        Guid productId,
        ProductMediaType mediaType,
        string filePathOrBlobKey,
        int sortOrder = 0,
        bool isPrimary = false,
        string? altText = null)
        : base(id)
    {
        ProductId = productId;
        MediaType = mediaType;
        FilePathOrBlobKey = filePathOrBlobKey ?? string.Empty;
        SortOrder = sortOrder;
        IsPrimary = isPrimary;
        AltText = altText;
    }
}
