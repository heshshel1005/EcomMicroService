using System;

namespace EcomMicroService.Catalog;

/// <summary>
/// Single review for display (PDP list or admin list).
/// </summary>
public class ProductReviewDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }
    /// <summary>Display name of the reviewer (e.g. from CustomerProfile or User).</summary>
    public string AuthorDisplayName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? ReviewText { get; set; }
    public ProductReviewStatus Status { get; set; }
    public DateTime CreationTime { get; set; }
}
