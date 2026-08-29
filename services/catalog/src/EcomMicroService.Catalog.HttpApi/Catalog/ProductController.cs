using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EcomMicroService.Catalog;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace EcomMicroService.Catalog;

[Route("api/catalog/product")]
[Area("catalog")]
public class ProductController : CatalogController
{
    private readonly IProductAppService _appService;

    public ProductController(IProductAppService appService)
    {
        _appService = appService;
    }

    [HttpGet("list")]
    public async Task<PagedResultDto<ProductListDto>> GetListAsync([FromQuery] ProductListRequestDto input)
    {
        return await _appService.GetListAsync(input);
    }

    [HttpGet("attribute-requirements-by-product-type")]
    public async Task<ProductTypeAttributeRequirementsDto> GetAttributeRequirementsByProductTypeAsync([FromQuery] Guid productTypeId)
    {
        return await _appService.GetAttributeRequirementsByProductTypeAsync(productTypeId);
    }

    [HttpGet("{id:guid}")]
    public async Task<ProductDto> GetAsync(Guid id)
    {
        return await _appService.GetAsync(id);
    }

    [HttpGet("attributes")]
    public async Task<List<ProductAttributeDto>> GetAttributesAsync()
    {
        return await _appService.GetAttributesAsync();
    }

    [HttpGet("product-types")]
    public async Task<List<ProductTypeDto>> GetProductTypesAsync()
    {
        return await _appService.GetProductTypesAsync();
    }

    [HttpPost]
    public async Task<ProductDto> CreateAsync([FromBody] CreateProductDto input)
    {
        return await _appService.CreateAsync(input);
    }

    [HttpPut("{id:guid}")]
    public async Task<ProductDto> UpdateAsync(Guid id, [FromBody] UpdateProductDto input)
    {
        return await _appService.UpdateAsync(id, input);
    }

    [HttpDelete("{id:guid}")]
    public async Task DeleteAsync(Guid id)
    {
        await _appService.DeleteAsync(id);
    }
}
