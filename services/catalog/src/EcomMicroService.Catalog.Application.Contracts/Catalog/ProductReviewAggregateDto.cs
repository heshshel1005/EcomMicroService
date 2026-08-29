namespace EcomMicroService.Catalog;

/// <summary>
/// Aggregate rating for a product (PDP and optional product cards).
/// </summary>
public class ProductReviewAggregateDto
{
    public double AverageRating { get; set; }
    public int TotalCount { get; set; }
}
