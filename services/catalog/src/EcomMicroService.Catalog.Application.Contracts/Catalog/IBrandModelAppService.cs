using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace EcomMicroService.Catalog;

/// <summary>
/// Application service contract for managing brand models (admin).
/// </summary>
public interface IBrandModelAppService : IApplicationService
{
    Task<List<BrandModelDto>> GetListAsync(Guid? brandId = null, bool? isActive = null);
    Task<List<BrandModelDto>> GetListByBrandIdAsync(Guid brandId);
    Task<BrandModelDto> GetAsync(Guid id);
    Task<BrandModelDto> CreateAsync(CreateBrandModelDto input);
    Task<BrandModelDto> UpdateAsync(Guid id, UpdateBrandModelDto input);
    Task DeleteAsync(Guid id);
}

