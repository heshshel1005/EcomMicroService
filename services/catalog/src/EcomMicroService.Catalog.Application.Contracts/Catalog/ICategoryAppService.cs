using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace EcomMicroService.Catalog;

/// <summary>
/// Application service for category CRUD and tree.
/// </summary>
public interface ICategoryAppService : IApplicationService
{
    /// <summary>
    /// Get the category tree (root nodes with nested children).
    /// </summary>
    Task<List<CategoryTreeDto>> GetTreeAsync();

    /// <summary>
    /// Get a flat list of all categories.
    /// </summary>
    Task<List<CategoryDto>> GetListAsync();

    /// <summary>
    /// Get a single category by id.
    /// </summary>
    Task<CategoryDto> GetAsync(Guid id);

    /// <summary>
    /// Create a category.
    /// </summary>
    Task<CategoryDto> CreateAsync(CreateCategoryDto input);

    /// <summary>
    /// Update a category.
    /// </summary>
    Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryDto input);

    /// <summary>
    /// Delete a category.
    /// </summary>
    Task DeleteAsync(Guid id);
}
