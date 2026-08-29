using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EcomMicroService.Catalog;

/// <summary>
/// Public API: get aggregate + list for PDP; authenticated customers submit review.
/// Exposed via ProductReviewController only (disable auto API to avoid ambiguous routes).
/// </summary>
[Volo.Abp.RemoteService(IsEnabled = false)]
public interface IProductReviewAppService : IApplicationService
{
    /// <summary>
    /// Get aggregate rating (average + count) for a product. Only approved reviews count.
    /// </summary>
    Task<ProductReviewAggregateDto> GetAggregateAsync(Guid productId);

    /// <summary>
    /// Get paged list of approved reviews for a product (for PDP).
    /// </summary>
    Task<PagedResultDto<ProductReviewDto>> GetListAsync(Guid productId, PagedAndSortedResultRequestDto input);

    /// <summary>
    /// Submit or update the current user's review for a product. Requires authentication.
    /// One review per user per product; submitting again updates the existing review (status reset to Pending if moderation is used).
    /// </summary>
    Task<ProductReviewDto> SubmitAsync(CreateProductReviewDto input);
}
