using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace EcomMicroService.Catalog;

public interface IProductTypeAttributeRuleAppService : IApplicationService
{
    Task<List<ProductTypeAttributeRuleDto>> GetListByProductTypeAsync(Guid productTypeId);
    Task ReplaceForProductTypeAsync(Guid productTypeId, List<UpdateProductTypeAttributeRuleDto> input);
}
