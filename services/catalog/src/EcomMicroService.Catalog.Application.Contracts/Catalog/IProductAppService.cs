using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EcomMicroService.Catalog;

/// <summary>
/// Application service for product CRUD (admin), variants, and inventory.
/// </summary>
public interface IProductAppService : IApplicationService
{
    Task<PagedResultDto<ProductListDto>> GetListAsync(ProductListRequestDto input);
    Task<ProductDto> GetAsync(Guid id);
    Task<List<ProductAttributeDto>> GetAttributesAsync();
    Task<List<ProductTypeDto>> GetProductTypesAsync();
    Task<ProductTypeAttributeRequirementsDto> GetAttributeRequirementsByProductTypeAsync(Guid productTypeId);
    Task<ProductDto> CreateAsync(CreateProductDto input);
    Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto input);
    Task DeleteAsync(Guid id);
}
