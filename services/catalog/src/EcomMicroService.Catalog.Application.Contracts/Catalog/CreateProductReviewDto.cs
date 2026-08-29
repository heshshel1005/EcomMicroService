using System;
using System.ComponentModel.DataAnnotations;

namespace EcomMicroService.Catalog;

/// <summary>
/// Input for customer to submit a review (rating + optional text).
/// </summary>
public class CreateProductReviewDto
{
    public Guid ProductId { get; set; }
    [Range(1, 5)]
    public int Rating { get; set; }
    [MaxLength(CatalogConsts.Catalog.ProductReviewMaxReviewTextLength)]
    public string? ReviewText { get; set; }
}
