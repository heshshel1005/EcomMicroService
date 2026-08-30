using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using EcomMicroService.Catalog.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Data;
using Volo.Abp.Localization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;

namespace EcomMicroService.Catalog;

public class CategoryAppService : CatalogAppService, ICategoryAppService
{
    private readonly IRepository<Category, Guid> _repository;
    private readonly ISettingProvider _settingProvider;
    private readonly IDataFilter<IMultiTenant> _multiTenantFilter;

    public CategoryAppService(
        IRepository<Category, Guid> repository,
        ISettingProvider settingProvider,
        IDataFilter<IMultiTenant> multiTenantFilter)
    {
        _repository = repository;
        _settingProvider = settingProvider;
        _multiTenantFilter = multiTenantFilter;
    }

    [Authorize(CatalogPermissions.Catalog.Default)]
    public async Task<List<CategoryTreeDto>> GetTreeAsync()
    {
        await CatalogTaxonomyAccess.EnsureCanReadAdminTaxonomyAsync(PermissionChecker);
        using (_multiTenantFilter.Disable())
        {
        var query = await _repository.WithDetailsAsync(x => x.Translations);
        var all = await AsyncExecuter.ToListAsync(query.WhereVisibleToTaxonomyReader(CurrentTenant));
        var defaultLanguage = await GetDefaultLanguageAsync();
        var dtos = all
            .Select(x => MapToDto(x, defaultLanguage))
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .ToList();
        return BuildTree(dtos, null);
        }
    }

    [Authorize(CatalogPermissions.Catalog.Default)]
    public async Task<List<CategoryDto>> GetListAsync()
    {
        await CatalogTaxonomyAccess.EnsureCanReadAdminTaxonomyAsync(PermissionChecker);
        using (_multiTenantFilter.Disable())
        {
        var query = await _repository.WithDetailsAsync(x => x.Translations);
        var all = await AsyncExecuter.ToListAsync(query.WhereVisibleToTaxonomyReader(CurrentTenant));
        var defaultLanguage = await GetDefaultLanguageAsync();
        return all
            .Select(x => MapToDto(x, defaultLanguage))
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .ToList();
        }
    }

    [Authorize(CatalogPermissions.Catalog.Default)]
    public async Task<CategoryDto> GetAsync(Guid id)
    {
        await CatalogTaxonomyAccess.EnsureCanReadAdminTaxonomyAsync(PermissionChecker);
        using (_multiTenantFilter.Disable())
        {
        var query = await _repository.WithDetailsAsync(x => x.Translations);
        var entity = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id).WhereVisibleToTaxonomyReader(CurrentTenant));
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(Category), id);
        }

        var defaultLanguage = await GetDefaultLanguageAsync();
        return MapToDto(entity, defaultLanguage);
        }
    }

    [Authorize]
    public async Task<CategoryDto> CreateAsync(CreateCategoryDto input)
    {
        await CatalogTaxonomyAccess.EnsureCanReadAdminTaxonomyAsync(PermissionChecker);
        await ValidateParentAsync(input.ParentId);
        var id = GuidGenerator.Create();
        var entity = new Category(id, input.Name, input.Slug, input.ParentId, input.DisplayOrder);
        var defaultLanguage = await GetDefaultLanguageAsync();
        entity.SetTranslations(
            input.Translations.Select(x => new CategoryTranslation(
                GuidGenerator.Create(),
                id,
                x.Language,
                x.Name)),
            defaultLanguage);
        await CatalogTaxonomyAccess.EnsureCanCreateTaxonomyInCurrentScopeAsync(PermissionChecker, CurrentTenant);
        await _repository.InsertAsync(entity);
        return MapToDto(entity, defaultLanguage);
    }

    [Authorize]
    public async Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryDto input)
    {
        await CatalogTaxonomyAccess.EnsureCanReadAdminTaxonomyAsync(PermissionChecker);
        using (_multiTenantFilter.Disable())
        {
        var query = await _repository.WithDetailsAsync(x => x.Translations);
        var entity = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id).WhereVisibleToTaxonomyReader(CurrentTenant));
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(Category), id);
        }

        await CatalogTaxonomyAccess.EnsureCanMutateTaxonomyEntityAsync(PermissionChecker, entity, CurrentTenant);
        if (input.ParentId == id)
            throw new Volo.Abp.BusinessException("ECommerce:CategoryCannotBeOwnParent");
        await ValidateParentAsync(input.ParentId);
        entity.Name = input.Name;
        entity.Slug = input.Slug;
        entity.ParentId = input.ParentId;
        entity.DisplayOrder = input.DisplayOrder;
        var defaultLanguage = await GetDefaultLanguageAsync();
        entity.SetTranslations(
            input.Translations.Select(x => new CategoryTranslation(
                GuidGenerator.Create(),
                id,
                x.Language,
                x.Name)),
            defaultLanguage);
        await _repository.UpdateAsync(entity);
        return MapToDto(entity, defaultLanguage);
        }
    }

    [Authorize]
    public async Task DeleteAsync(Guid id)
    {
        await CatalogTaxonomyAccess.EnsureCanReadAdminTaxonomyAsync(PermissionChecker);
        var entity = await CatalogTaxonomyAccess.GetVisibleEntityAsync(
            _repository,
            id,
            _multiTenantFilter,
            CurrentTenant,
            AsyncExecuter);
        await CatalogTaxonomyAccess.EnsureCanMutateTaxonomyEntityAsync(PermissionChecker, entity, CurrentTenant);
        var children = await CatalogTaxonomyAccess.GetVisibleListAsync(
            _repository,
            _multiTenantFilter,
            CurrentTenant,
            AsyncExecuter,
            x => x.ParentId == id);
        if (children.Count > 0)
            throw new Volo.Abp.BusinessException("ECommerce:CategoryHasChildren");
        await _repository.DeleteAsync(id);
    }

    private async Task ValidateParentAsync(Guid? parentId)
    {
        if (parentId == null) return;
        await CatalogTaxonomyAccess.GetVisibleEntityAsync(
            _repository,
            parentId.Value,
            _multiTenantFilter,
            CurrentTenant,
            AsyncExecuter);
    }

    private static List<CategoryTreeDto> BuildTree(List<CategoryDto> flat, Guid? parentId, HashSet<Guid>? visited = null)
    {
        visited ??= new HashSet<Guid>();
        return flat
            .Where(x => x.ParentId == parentId)
            .Select(dto =>
            {
                var children = visited.Add(dto.Id)
                    ? BuildTree(flat, dto.Id, visited)
                    : new List<CategoryTreeDto>();
                return new CategoryTreeDto
                {
                    Id = dto.Id,
                    ParentId = dto.ParentId,
                    Name = dto.Name,
                    Slug = dto.Slug,
                    DisplayOrder = dto.DisplayOrder,
                    Children = children
                };
            })
            .ToList();
    }

    private async Task<string> GetDefaultLanguageAsync()
    {
        var configured = await _settingProvider.GetOrNullAsync(LocalizationSettingNames.DefaultLanguage);
        return string.IsNullOrWhiteSpace(configured) ? "en" : configured;
    }

    private static CategoryDto MapToDto(Category entity, string? defaultLanguage)
    {
        var resolved = CatalogTranslationResolver.Resolve(
            entity.Translations,
            CultureInfo.CurrentUICulture.Name,
            defaultLanguage);
        return new CategoryDto
        {
            Id = entity.Id,
            ParentId = entity.ParentId,
            Name = resolved?.Name ?? entity.Name,
            Slug = entity.Slug,
            DisplayOrder = entity.DisplayOrder,
            Translations = entity.Translations
                .Select(x => new CategoryTranslationDto
                {
                    Language = x.Language,
                    Name = x.Name
                })
                .OrderBy(x => x.Language)
                .ToList()
        };
    }
}
