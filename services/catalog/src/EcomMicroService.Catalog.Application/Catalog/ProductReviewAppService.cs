using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace EcomMicroService.Catalog;

[RemoteService(IsEnabled = false)]
public class ProductReviewAppService : CatalogAppService, IProductReviewAppService
{
    private readonly IRepository<ProductReview, Guid> _reviewRepository;
    private readonly IRepository<Product, Guid> _productRepository;

    public ProductReviewAppService(
        IRepository<ProductReview, Guid> reviewRepository,
        IRepository<Product, Guid> productRepository)
    {
        _reviewRepository = reviewRepository;
        _productRepository = productRepository;
    }

    [AllowAnonymous]
    public async Task<ProductReviewAggregateDto> GetAggregateAsync(Guid productId)
    {
        var query = await _reviewRepository.GetQueryableAsync();
        query = query.Where(x => x.ProductId == productId && x.Status == ProductReviewStatus.Approved);
        var list = await AsyncExecuter.ToListAsync(query);
        var count = list.Count;
        if (count == 0)
            return new ProductReviewAggregateDto { AverageRating = 0, TotalCount = 0 };
        var avg = list.Average(x => x.Rating);
        return new ProductReviewAggregateDto { AverageRating = Math.Round(avg, 2), TotalCount = count };
    }

    [AllowAnonymous]
    public async Task<PagedResultDto<ProductReviewDto>> GetListAsync(Guid productId, PagedAndSortedResultRequestDto input)
    {
        var query = await _reviewRepository.GetQueryableAsync();
        query = query.Where(x => x.ProductId == productId && x.Status == ProductReviewStatus.Approved);
        var total = await AsyncExecuter.CountAsync(query);
        var sorting = input.Sorting ?? nameof(ProductReview.CreationTime) + " DESC";
        var sortDesc = sorting.EndsWith(" DESC", StringComparison.OrdinalIgnoreCase);
        var sortKey = sorting.Replace(" DESC", "", StringComparison.OrdinalIgnoreCase).Trim();
        query = sortKey switch
        {
            nameof(ProductReview.Rating) => sortDesc ? query.OrderByDescending(x => x.Rating) : query.OrderBy(x => x.Rating),
            nameof(ProductReview.CreationTime) => sortDesc ? query.OrderByDescending(x => x.CreationTime) : query.OrderBy(x => x.CreationTime),
            _ => query.OrderByDescending(x => x.CreationTime)
        };
        var skip = input.SkipCount;
        var take = input.MaxResultCount > 0 ? input.MaxResultCount : 10;
        var reviews = await AsyncExecuter.ToListAsync(query.Skip(skip).Take(take));
        var dtos = await MapToDtosAsync(reviews);
        return new PagedResultDto<ProductReviewDto>(total, dtos);
    }

    [Authorize]
    public async Task<ProductReviewDto> SubmitAsync(CreateProductReviewDto input)
    {
        var userId = CurrentUser.Id ?? throw new Volo.Abp.Authorization.AbpAuthorizationException("User must be logged in to submit a review.");
        var product = await _productRepository.FirstOrDefaultAsync(p => p.Id == input.ProductId);
        if (product == null)
            throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(Product), input.ProductId);

        var existing = await _reviewRepository.FirstOrDefaultAsync(x => x.ProductId == input.ProductId && x.UserId == userId);
        if (existing != null)
        {
            existing.Rating = input.Rating;
            existing.ReviewText = input.ReviewText?.Trim();
            existing.Status = ProductReviewStatus.Pending;
            await _reviewRepository.UpdateAsync(existing);
            return await MapToDtoAsync(existing);
        }

        var review = new ProductReview(
            GuidGenerator.Create(),
            input.ProductId,
            userId,
            input.Rating,
            input.ReviewText?.Trim(),
            ProductReviewStatus.Pending);
        await _reviewRepository.InsertAsync(review);
        return await MapToDtoAsync(review);
    }

    private async Task<ProductReviewDto> MapToDtoAsync(ProductReview r)
    {
        var dto = new ProductReviewDto
        {
            Id = r.Id,
            ProductId = r.ProductId,
            UserId = r.UserId,
            Rating = r.Rating,
            ReviewText = r.ReviewText,
            Status = r.Status,
            CreationTime = r.CreationTime
        };
        dto.AuthorDisplayName = await GetDisplayNameAsync(r.UserId);
        return dto;
    }

    private Task<string> GetDisplayNameAsync(Guid userId)
    {
        if (CurrentUser.Id == userId && !string.IsNullOrWhiteSpace(CurrentUser.Name))
        {
            return Task.FromResult(CurrentUser.Name!);
        }

        if (CurrentUser.Id == userId && !string.IsNullOrWhiteSpace(CurrentUser.UserName))
        {
            return Task.FromResult(CurrentUser.UserName!);
        }

        return Task.FromResult("Customer");
    }

    private async Task<System.Collections.Generic.List<ProductReviewDto>> MapToDtosAsync(System.Collections.Generic.List<ProductReview> reviews)
    {
        var list = new System.Collections.Generic.List<ProductReviewDto>();
        foreach (var r in reviews)
            list.Add(await MapToDtoAsync(r));
        return list;
    }
}
