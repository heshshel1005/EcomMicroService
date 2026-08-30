using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EcomMicroService.Catalog;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace EcomMicroService.Catalog;

[RemoteService(Name = CatalogRemoteServiceConsts.RemoteServiceName)]
[Route("api/catalog/category")]
[Area("catalog")]
public class CategoryController : CatalogController
{
    private readonly ICategoryAppService _appService;

    public CategoryController(ICategoryAppService appService)
    {
        _appService = appService;
    }

    /// <summary>
    /// Get the category tree (root nodes with nested children). Define route before {id} so "tree" is not matched as id.
    /// </summary>
    [HttpGet("tree")]
    public async Task<List<CategoryTreeDto>> GetTreeAsync()
    {
        return await _appService.GetTreeAsync();
    }

    /// <summary>
    /// Get a flat list of all categories.
    /// </summary>
    [HttpGet]
    [HttpGet("list")]
    public async Task<List<CategoryDto>> GetListAsync()
    {
        return await _appService.GetListAsync();
    }

    /// <summary>
    /// Get a single category by id.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<CategoryDto> GetAsync(Guid id)
    {
        return await _appService.GetAsync(id);
    }

    /// <summary>
    /// Create a category.
    /// </summary>
    [HttpPost]
    public async Task<CategoryDto> CreateAsync([FromBody] CreateCategoryDto input)
    {
        return await _appService.CreateAsync(input);
    }

    /// <summary>
    /// Update a category.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<CategoryDto> UpdateAsync(Guid id, [FromBody] UpdateCategoryDto input)
    {
        return await _appService.UpdateAsync(id, input);
    }

    /// <summary>
    /// Delete a category.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task DeleteAsync(Guid id)
    {
        await _appService.DeleteAsync(id);
    }
}
