using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace EcomMicroService.Catalog;

public interface IProductTypeAppService : IApplicationService
{
    Task<List<ProductTypeDto>> GetListAsync(bool? isActive = null);
    Task<ProductTypeDto> GetAsync(Guid id);
    Task<ProductTypeDto> CreateAsync(CreateProductTypeDto input);
    Task<ProductTypeDto> UpdateAsync(Guid id, UpdateProductTypeDto input);
    Task DeleteAsync(Guid id);
}
