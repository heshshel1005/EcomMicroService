using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using EcomMicroService.Catalog.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Localization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Settings;

namespace EcomMicroService.Catalog;

public class BrandModelAppService : CatalogAppService, IBrandModelAppService
{
    private readonly IRepository<BrandModel, Guid> _repository;
    private readonly IRepository<Brand, Guid> _brandRepository;
    private readonly ISettingProvider _settingProvider;

    public BrandModelAppService(
        IRepository<BrandModel, Guid> repository,
        IRepository<Brand, Guid> brandRepository,
        ISettingProvider settingProvider)
    {
        _repository = repository;
        _brandRepository = brandRepository;
        _settingProvider = settingProvider;
    }

    // Read endpoints are available to any Catalog permission (tenant-only).
    // Management (create/update/delete) remains BrandModels-only.
    [Authorize(CatalogPermissions.Catalog.Default)]
    public async Task<List<BrandModelDto>> GetListAsync(Guid? brandId = null, bool? isActive = null)
    {
        var query = await _repository.WithDetailsAsync(x => x.Translations);
        query = query.Where(x =>
            (!brandId.HasValue || x.BrandId == brandId.Value) &&
            (!isActive.HasValue || x.IsActive == isActive.Value));
        var list = await AsyncExecuter.ToListAsync(query);
        var defaultLanguage = await GetDefaultLanguageAsync();

        return list
            .Select(x => MapToDto(x, defaultLanguage))
            .OrderBy(x => x.Name)
            .ToList();
    }

    [Authorize(CatalogPermissions.Catalog.Default)]
    public async Task<List<BrandModelDto>> GetListByBrandIdAsync(Guid brandId)
    {
        // Ensure brand exists to give a clearer error if an invalid id is used.
        await _brandRepository.GetAsync(brandId);

        var query = await _repository.WithDetailsAsync(x => x.Translations);
        query = query.Where(x => x.BrandId == brandId && x.IsActive);
        var list = await AsyncExecuter.ToListAsync(query);
        var defaultLanguage = await GetDefaultLanguageAsync();
        return list
            .Select(x => MapToDto(x, defaultLanguage))
            .OrderBy(x => x.Name)
            .ToList();
    }

    [Authorize(CatalogPermissions.Catalog.Default)]
    public async Task<BrandModelDto> GetAsync(Guid id)
    {
        var query = await _repository.WithDetailsAsync(x => x.Translations);
        var entity = await AsyncExecuter.FirstAsync(query.Where(x => x.Id == id));
        var defaultLanguage = await GetDefaultLanguageAsync();
        return MapToDto(entity, defaultLanguage);
    }

    [Authorize(CatalogPermissions.Catalog.BrandModels)]
    public async Task<BrandModelDto> CreateAsync(CreateBrandModelDto input)
    {
        await _brandRepository.GetAsync(input.BrandId);

        var id = GuidGenerator.Create();
        var entity = new BrandModel(
            id,
            input.BrandId,
            input.Name,
            input.Code,
            input.IsActive);
        var defaultLanguage = await GetDefaultLanguageAsync();
        entity.SetTranslations(
            input.Translations.Select(x => new BrandModelTranslation(
                GuidGenerator.Create(),
                id,
                x.Language,
                x.Name)),
            defaultLanguage);

        await _repository.InsertAsync(entity);
        return MapToDto(entity, defaultLanguage);
    }

    [Authorize(CatalogPermissions.Catalog.BrandModels)]
    public async Task<BrandModelDto> UpdateAsync(Guid id, UpdateBrandModelDto input)
    {
        await _brandRepository.GetAsync(input.BrandId);

        var query = await _repository.WithDetailsAsync(x => x.Translations);
        var entity = await AsyncExecuter.FirstAsync(query.Where(x => x.Id == id));
        entity.BrandId = input.BrandId;
        entity.Name = input.Name;
        entity.Code = input.Code;
        entity.IsActive = input.IsActive;
        var defaultLanguage = await GetDefaultLanguageAsync();
        entity.SetTranslations(
            input.Translations.Select(x => new BrandModelTranslation(
                GuidGenerator.Create(),
                id,
                x.Language,
                x.Name)),
            defaultLanguage);

        await _repository.UpdateAsync(entity);
        return MapToDto(entity, defaultLanguage);
    }

    [Authorize(CatalogPermissions.Catalog.BrandModels)]
    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }

    private async Task<string> GetDefaultLanguageAsync()
    {
        var configured = await _settingProvider.GetOrNullAsync(LocalizationSettingNames.DefaultLanguage);
        return string.IsNullOrWhiteSpace(configured) ? "en" : configured;
    }

    private static BrandModelDto MapToDto(BrandModel entity, string? defaultLanguage)
    {
        var resolved = CatalogTranslationResolver.Resolve(
            entity.Translations,
            CultureInfo.CurrentUICulture.Name,
            defaultLanguage);
        return new BrandModelDto
        {
            Id = entity.Id,
            BrandId = entity.BrandId,
            Name = resolved?.Name ?? entity.Name,
            Code = entity.Code,
            IsActive = entity.IsActive,
            Translations = entity.Translations
                .Select(x => new BrandModelTranslationDto
                {
                    Language = x.Language,
                    Name = x.Name
                })
                .OrderBy(x => x.Language)
                .ToList()
        };
    }
}

