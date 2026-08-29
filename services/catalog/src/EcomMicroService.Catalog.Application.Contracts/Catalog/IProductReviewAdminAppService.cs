using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EcomMicroService.Catalog;

/// <summary>
/// Admin API: list all reviews (with filters), approve, reject, delete.
/// Exposed via ProductReviewAdminController only (disable auto API to avoid ambiguous routes).
/// </summary>
[Volo.Abp.RemoteService(IsEnabled = false)]
public interface IProductReviewAdminAppService : IApplicationService
{
    Task<PagedResultDto<ProductReviewDto>> GetListAsync(ProductReviewListRequestDto input);
    Task ApproveAsync(Guid id);
    Task RejectAsync(Guid id);
    Task DeleteAsync(Guid id);
}
