using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using EcomMicroService.Catalog.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Localization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Settings;

namespace EcomMicroService.Catalog;

public class BrandAppService : CatalogAppService, IBrandAppService
{
    private readonly IRepository<Brand, Guid> _repository;
    private readonly ISettingProvider _settingProvider;

    public BrandAppService(
        IRepository<Brand, Guid> repository,
        ISettingProvider settingProvider)
    {
        _repository = repository;
        _settingProvider = settingProvider;
    }

    // Read endpoints are available to any Catalog permission (tenant-only).
    // Management (create/update/delete) remains Brands-only.
    [Authorize(CatalogPermissions.Catalog.Default)]
    public async Task<List<BrandDto>> GetListAsync(bool? isActive = null)
    {
        var query = await _repository.WithDetailsAsync(x => x.Translations);
        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        var list = await AsyncExecuter.ToListAsync(query);
        var defaultLanguage = await GetDefaultLanguageAsync();

        return list
            .Select(x => MapToDto(x, defaultLanguage))
            .OrderBy(x => x.Name)
            .ToList();
    }

    [Authorize(CatalogPermissions.Catalog.Default)]
    public async Task<BrandDto> GetAsync(Guid id)
    {
        var query = await _repository.WithDetailsAsync(x => x.Translations);
        var entity = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id));
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(Brand), id);
        }

        var defaultLanguage = await GetDefaultLanguageAsync();
        return MapToDto(entity, defaultLanguage);
    }

    [Authorize(CatalogPermissions.Catalog.Brands)]
    public async Task<BrandDto> CreateAsync(CreateBrandDto input)
    {
        var id = GuidGenerator.Create();
        var entity = new Brand(
            id,
            input.Name,
            input.Slug,
            input.Description,
            input.IsActive);
        var defaultLanguage = await GetDefaultLanguageAsync();
        entity.SetTranslations(
            input.Translations.Select(x => new BrandTranslation(
                GuidGenerator.Create(),
                id,
                x.Language,
                x.Name,
                x.Description)),
            defaultLanguage);

        await _repository.InsertAsync(entity);
        return MapToDto(entity, defaultLanguage);
    }

    [Authorize(CatalogPermissions.Catalog.Brands)]
    public async Task<BrandDto> UpdateAsync(Guid id, UpdateBrandDto input)
    {
        var query = await _repository.WithDetailsAsync(x => x.Translations);
        var entity = await AsyncExecuter.FirstAsync(query.Where(x => x.Id == id));

        entity.Name = input.Name;
        entity.Slug = input.Slug;
        entity.Description = input.Description;
        entity.IsActive = input.IsActive;
        var defaultLanguage = await GetDefaultLanguageAsync();
        entity.SetTranslations(
            input.Translations.Select(x => new BrandTranslation(
                GuidGenerator.Create(),
                id,
                x.Language,
                x.Name,
                x.Description)),
            defaultLanguage);

        await _repository.UpdateAsync(entity);
        return MapToDto(entity, defaultLanguage);
    }

    [Authorize(CatalogPermissions.Catalog.Brands)]
    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }

    private async Task<string> GetDefaultLanguageAsync()
    {
        var configured = await _settingProvider.GetOrNullAsync(LocalizationSettingNames.DefaultLanguage);
        return string.IsNullOrWhiteSpace(configured) ? "en" : configured;
    }

    private static BrandDto MapToDto(Brand entity, string? defaultLanguage)
    {
        var resolved = CatalogTranslationResolver.Resolve(
            entity.Translations,
            CultureInfo.CurrentUICulture.Name,
            defaultLanguage);
        return new BrandDto
        {
            Id = entity.Id,
            Name = resolved?.Name ?? entity.Name,
            Slug = entity.Slug,
            Description = resolved?.Description ?? entity.Description,
            IsActive = entity.IsActive,
            Translations = entity.Translations
                .Select(x => new BrandTranslationDto
                {
                    Language = x.Language,
                    Name = x.Name,
                    Description = x.Description
                })
                .OrderBy(x => x.Language)
                .ToList()
        };
    }
}

