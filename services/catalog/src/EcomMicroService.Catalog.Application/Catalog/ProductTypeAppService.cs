using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;

namespace EcomMicroService.Catalog;

public class ProductTypeAppService : CatalogAppService, IProductTypeAppService
{
    private readonly IRepository<ProductType, Guid> _repository;
    private readonly ISettingProvider _settingProvider;
    private readonly IDataFilter<IMultiTenant> _multiTenantFilter;

    public ProductTypeAppService(
        IRepository<ProductType, Guid> repository,
        ISettingProvider settingProvider,
        IDataFilter<IMultiTenant> multiTenantFilter)
    {
        _repository = repository;
        _settingProvider = settingProvider;
        _multiTenantFilter = multiTenantFilter;
    }

    [Authorize]
    public async Task<List<ProductTypeDto>> GetListAsync(bool? isActive = null)
    {
        await CatalogTaxonomyAccess.EnsureCanReadAdminTaxonomyAsync(PermissionChecker);
        using (_multiTenantFilter.Disable())
        {
            var query = await _repository.WithDetailsAsync(x => x.Translations);
            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            query = query.WhereVisibleToTaxonomyReader(CurrentTenant);
            var list = await AsyncExecuter.ToListAsync(query);
            var defaultLanguage = await GetDefaultLanguageAsync();

            return list
                .Select(x => MapToDto(x, defaultLanguage))
                .OrderBy(x => x.Name)
                .ToList();
        }
    }

    [Authorize]
    public async Task<ProductTypeDto> GetAsync(Guid id)
    {
        await CatalogTaxonomyAccess.EnsureCanReadAdminTaxonomyAsync(PermissionChecker);
        using (_multiTenantFilter.Disable())
        {
            var query = await _repository.WithDetailsAsync(x => x.Translations);
            var entity = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id).WhereVisibleToTaxonomyReader(CurrentTenant));
            if (entity == null)
            {
                throw new EntityNotFoundException(typeof(ProductType), id);
            }

            var defaultLanguage = await GetDefaultLanguageAsync();
            return MapToDto(entity, defaultLanguage);
        }
    }

    [Authorize]
    public async Task<ProductTypeDto> CreateAsync(CreateProductTypeDto input)
    {
        await CatalogTaxonomyAccess.EnsureCanReadAdminTaxonomyAsync(PermissionChecker);
        var id = GuidGenerator.Create();
        var entity = new ProductType(
            id,
            input.Code,
            input.Name,
            input.IsActive);
        var defaultLanguage = await GetDefaultLanguageAsync();
        entity.SetTranslations(
            input.Translations.Select(x => new ProductTypeTranslation(
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
    public async Task<ProductTypeDto> UpdateAsync(Guid id, UpdateProductTypeDto input)
    {
        await CatalogTaxonomyAccess.EnsureCanReadAdminTaxonomyAsync(PermissionChecker);
        using (_multiTenantFilter.Disable())
        {
            var query = await _repository.WithDetailsAsync(x => x.Translations);
            var entity = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id).WhereVisibleToTaxonomyReader(CurrentTenant));
            if (entity == null)
            {
                throw new EntityNotFoundException(typeof(ProductType), id);
            }

            await CatalogTaxonomyAccess.EnsureCanMutateTaxonomyEntityAsync(PermissionChecker, entity, CurrentTenant);

            entity.SetCode(input.Code);
            entity.SetName(input.Name);
            entity.SetIsActive(input.IsActive);
            var defaultLanguage = await GetDefaultLanguageAsync();
            entity.SetTranslations(
                input.Translations.Select(x => new ProductTypeTranslation(
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
        await _repository.DeleteAsync(id);
    }

    private async Task<string> GetDefaultLanguageAsync()
    {
        var configured = await _settingProvider.GetOrNullAsync(LocalizationSettingNames.DefaultLanguage);
        return string.IsNullOrWhiteSpace(configured) ? "en" : configured;
    }

    private static ProductTypeDto MapToDto(ProductType entity, string? defaultLanguage)
    {
        var resolved = CatalogTranslationResolver.Resolve(
            entity.Translations,
            CultureInfo.CurrentUICulture.Name,
            defaultLanguage);
        return new ProductTypeDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = resolved?.Name ?? entity.Name,
            IsActive = entity.IsActive,
            Translations = entity.Translations
                .Select(x => new ProductTypeTranslationDto
                {
                    Language = x.Language,
                    Name = x.Name
                })
                .OrderBy(x => x.Language)
                .ToList()
        };
    }
}
