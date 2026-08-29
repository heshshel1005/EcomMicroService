using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Catalog;

/// <summary>
/// Customer review/rating for a product. One rating (1-5) and optional text per user per product.
/// Moderation: Pending → Approved (visible on PDP) or Rejected.
/// </summary>
public class ProductReview : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }
    /// <summary>Star rating 1-5.</summary>
    public int Rating { get; set; }
    public string? ReviewText { get; set; }
    public ProductReviewStatus Status { get; set; }

    protected ProductReview()
    {
    }

    public ProductReview(
        Guid id,
        Guid productId,
        Guid userId,
        int rating,
        string? reviewText = null,
        ProductReviewStatus status = ProductReviewStatus.Pending)
        : base(id)
    {
        ProductId = productId;
        UserId = userId;
        Rating = rating;
        ReviewText = reviewText;
        Status = status;
    }
}
