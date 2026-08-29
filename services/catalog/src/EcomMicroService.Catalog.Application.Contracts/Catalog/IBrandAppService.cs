using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace EcomMicroService.Catalog;

/// <summary>
/// Application service contract for managing brands (admin).
/// </summary>
public interface IBrandAppService : IApplicationService
{
    Task<List<BrandDto>> GetListAsync(bool? isActive = null);
    Task<BrandDto> GetAsync(Guid id);
    Task<BrandDto> CreateAsync(CreateBrandDto input);
    Task<BrandDto> UpdateAsync(Guid id, UpdateBrandDto input);
    Task DeleteAsync(Guid id);
}

