using System;

namespace EcomMicroService.Catalog;

/// <summary>
/// DTO for product media (image or video) metadata. File content is served via GET product-media/{id}/file.
/// </summary>
public class ProductMediaDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public ProductMediaType MediaType { get; set; }
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }
    public string? AltText { get; set; }
    public DateTime CreationTime { get; set; }
}
