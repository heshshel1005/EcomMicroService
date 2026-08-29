using System;
using Volo.Abp.Application.Dtos;

namespace EcomMicroService.Catalog;

/// <summary>
/// Request for listing reviews (public: by product; admin: optional product filter, status filter).
/// </summary>
public class ProductReviewListRequestDto : PagedAndSortedResultRequestDto
{
    public Guid? ProductId { get; set; }
    public ProductReviewStatus? Status { get; set; }
}
