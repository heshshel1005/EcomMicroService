using System;

namespace EcomMicroService.Catalog;

/// <summary>
/// Lightweight media item for PDP gallery (image or video). Used in ProductDto.Media.
/// </summary>
public class ProductMediaItemDto
{
    public Guid Id { get; set; }
    public ProductMediaType MediaType { get; set; }
}
