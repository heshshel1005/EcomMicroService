using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using EcomMicroService.Catalog.Localization;
using EcomMicroService.Catalog.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;

namespace EcomMicroService.Catalog;

public class AttributeDefinitionAppService : CatalogAppService, IAttributeDefinitionAppService
{
    private readonly IRepository<AttributeDefinition, Guid> _repository;
    private readonly IRepository<AttributeDefinitionTranslation, Guid> _translationRepository;
    private readonly IRepository<AttributeOption, Guid> _attributeOptionRepository;
    private readonly IRepository<AttributeOptionTranslation, Guid> _optionTranslationRepository;
    private readonly ISettingProvider _settingProvider;
    private readonly IDataFilter<IMultiTenant> _multiTenantFilter;

    public AttributeDefinitionAppService(
        IRepository<AttributeDefinition, Guid> repository,
        IRepository<AttributeDefinitionTranslation, Guid> translationRepository,
        IRepository<AttributeOption, Guid> attributeOptionRepository,
        IRepository<AttributeOptionTranslation, Guid> optionTranslationRepository,
        ISettingProvider settingProvider,
        IDataFilter<IMultiTenant> multiTenantFilter)
    {
        _repository = repository;
        _translationRepository = translationRepository;
        _attributeOptionRepository = attributeOptionRepository;
        _optionTranslationRepository = optionTranslationRepository;
        _settingProvider = settingProvider;
        _multiTenantFilter = multiTenantFilter;
    }

    [Authorize]
    public async Task<List<AttributeDefinitionDto>> GetListAsync()
    {
        await CatalogTaxonomyAccess.EnsureCanReadAdminTaxonomyAsync(PermissionChecker);
        var list = await CatalogTaxonomyAccess.GetVisibleListAsync(
            _repository,
            _multiTenantFilter,
            CurrentTenant,
            AsyncExecuter);
        var ids = list.Select(x => x.Id).ToList();
        var translations = ids.Count == 0
            ? new List<AttributeDefinitionTranslation>()
            : await GetDefinitionTranslationsForDefinitionIdsAsync(ids);
        var byDef = translations
            .GroupBy(x => x.AttributeDefinitionId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<AttributeDefinitionTranslation>)g.ToList());
        var defaultLanguage = await GetDefaultLanguageAsync();

        return list
            .OrderBy(x => x.Key)
            .Select(x => MapToDto(
                x,
                byDef.TryGetValue(x.Id, out var tr) ? tr : Array.Empty<AttributeDefinitionTranslation>(),
                defaultLanguage))
            .ToList();
    }

    [Authorize]
    public async Task<AttributeDefinitionDto> GetAsync(Guid id)
    {
        await CatalogTaxonomyAccess.EnsureCanReadAdminTaxonomyAsync(PermissionChecker);
        var entity = await CatalogTaxonomyAccess.GetVisibleEntityAsync(
            _repository,
            id,
            _multiTenantFilter,
            CurrentTenant,
            AsyncExecuter);
        var translations = await GetDefinitionTranslationsForDefinitionIdsAsync(new List<Guid> { id });
        var defaultLanguage = await GetDefaultLanguageAsync();
        return MapToDto(entity, translations, defaultLanguage);
    }

    [Authorize]
    public async Task<AttributeDefinitionDto> CreateAsync(CreateAttributeDefinitionDto input)
    {
        var defaultLanguage = await GetDefaultLanguageAsync();
        ValidateDefinitionTranslations(input.Translations, defaultLanguage);

        var entity = new AttributeDefinition(
            GuidGenerator.Create(),
            input.Key,
            input.DataType,
            input.AllowedValuesJson,
            input.RegexPattern,
            input.MinValue,
            input.MaxValue,
            input.IsRequired,
            input.IsRecommended);

        await CatalogTaxonomyAccess.EnsureCanCreateTaxonomyInCurrentScopeAsync(PermissionChecker, CurrentTenant);
        await _repository.InsertAsync(entity);
        await SyncAttributeOptionsAsync(entity);
        await ReplaceDefinitionTranslationsAsync(entity.Id, entity.TenantId, input.Translations);
        return await GetAsync(entity.Id);
    }

    [Authorize]
    public async Task<AttributeDefinitionDto> UpdateAsync(Guid id, UpdateAttributeDefinitionDto input)
    {
        var defaultLanguage = await GetDefaultLanguageAsync();
        ValidateDefinitionTranslations(input.Translations, defaultLanguage);

        var entity = await CatalogTaxonomyAccess.GetVisibleEntityAsync(
            _repository,
            id,
            _multiTenantFilter,
            CurrentTenant,
            AsyncExecuter);
        await CatalogTaxonomyAccess.EnsureCanMutateTaxonomyEntityAsync(PermissionChecker, entity, CurrentTenant);
        if (entity.GovernanceStatus == AttributeDefinitionGovernanceStatus.Archived)
        {
            throw new BusinessException(CatalogDomainErrorCodes.AttributeDefinitionArchivedMutationBlocked);
        }

        entity.SetKey(input.Key);
        entity.SetDataType(input.DataType);
        entity.SetAllowedValuesJson(input.AllowedValuesJson);
        entity.SetRegexPattern(input.RegexPattern);
        entity.SetRange(input.MinValue, input.MaxValue);
        entity.SetRequirementFlags(input.IsRequired, input.IsRecommended);

        await _repository.UpdateAsync(entity);
        await SyncAttributeOptionsAsync(entity);
        await ReplaceDefinitionTranslationsAsync(id, entity.TenantId, input.Translations);
        return await GetAsync(id);
    }

    [Authorize]
    public async Task<AttributeDefinitionDto> SubmitForReviewAsync(Guid id)
    {
        await CatalogTaxonomyAccess.EnsureCanReadAdminTaxonomyAsync(PermissionChecker);
        var entity = await CatalogTaxonomyAccess.GetVisibleEntityAsync(
            _repository,
            id,
            _multiTenantFilter,
            CurrentTenant,
            AsyncExecuter);
        await CatalogTaxonomyAccess.EnsureCanMutateTaxonomyEntityAsync(PermissionChecker, entity, CurrentTenant);
        entity.SubmitForReview();
        await _repository.UpdateAsync(entity);
        return await GetAsync(id);
    }

    [Authorize(CatalogPermissions.Catalog.AttributeDefinitionsReview)]
    public async Task<AttributeDefinitionDto> RejectReviewAsync(Guid id)
    {
        await CatalogTaxonomyAccess.EnsureCanReadAdminTaxonomyAsync(PermissionChecker);
        var entity = await CatalogTaxonomyAccess.GetVisibleEntityAsync(
            _repository,
            id,
            _multiTenantFilter,
            CurrentTenant,
            AsyncExecuter);
        await CatalogTaxonomyAccess.EnsureCanMutateTaxonomyEntityAsync(PermissionChecker, entity, CurrentTenant);
        entity.RejectReview();
        await _repository.UpdateAsync(entity);
        return await GetAsync(id);
    }

    [Authorize(CatalogPermissions.Catalog.AttributeDefinitionsPublish)]
    public async Task<AttributeDefinitionDto> PublishAsync(Guid id)
    {
        await CatalogTaxonomyAccess.EnsureCanReadAdminTaxonomyAsync(PermissionChecker);
        var entity = await CatalogTaxonomyAccess.GetVisibleEntityAsync(
            _repository,
            id,
            _multiTenantFilter,
            CurrentTenant,
            AsyncExecuter);
        await CatalogTaxonomyAccess.EnsureCanMutateTaxonomyEntityAsync(PermissionChecker, entity, CurrentTenant);
        entity.Publish();
        await _repository.UpdateAsync(entity);
        return await GetAsync(id);
    }

    [Authorize(CatalogPermissions.Catalog.AttributeDefinitionsPublish)]
    public async Task<AttributeDefinitionDto> ArchiveAsync(Guid id)
    {
        await CatalogTaxonomyAccess.EnsureCanReadAdminTaxonomyAsync(PermissionChecker);
        var entity = await CatalogTaxonomyAccess.GetVisibleEntityAsync(
            _repository,
            id,
            _multiTenantFilter,
            CurrentTenant,
            AsyncExecuter);
        await CatalogTaxonomyAccess.EnsureCanMutateTaxonomyEntityAsync(PermissionChecker, entity, CurrentTenant);
        entity.Archive();
        await _repository.UpdateAsync(entity);
        return await GetAsync(id);
    }

    [Authorize(CatalogPermissions.Catalog.AttributeDefinitionsPublish)]
    public async Task<AttributeDefinitionDto> DemoteToDraftAsync(Guid id)
    {
        await CatalogTaxonomyAccess.EnsureCanReadAdminTaxonomyAsync(PermissionChecker);
        var entity = await CatalogTaxonomyAccess.GetVisibleEntityAsync(
            _repository,
            id,
            _multiTenantFilter,
            CurrentTenant,
            AsyncExecuter);
        await CatalogTaxonomyAccess.EnsureCanMutateTaxonomyEntityAsync(PermissionChecker, entity, CurrentTenant);
        entity.DemoteToDraft();
        await _repository.UpdateAsync(entity);
        return await GetAsync(id);
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

    [Authorize]
    public async Task<List<AttributeOptionTranslationDto>> GetOptionTranslationsAsync(Guid attributeDefinitionId)
    {
        await CatalogTaxonomyAccess.EnsureCanReadAdminTaxonomyAsync(PermissionChecker);
        await CatalogTaxonomyAccess.GetVisibleEntityAsync(
            _repository,
            attributeDefinitionId,
            _multiTenantFilter,
            CurrentTenant,
            AsyncExecuter);
        return await BuildOptionTranslationDtosAsync(attributeDefinitionId);
    }

    [Authorize]
    public async Task<List<AttributeOptionTranslationDto>> SaveOptionTranslationsAsync(
        Guid attributeDefinitionId,
        SaveAttributeOptionTranslationsDto input)
    {
        await CatalogTaxonomyAccess.EnsureCanReadAdminTaxonomyAsync(PermissionChecker);
        var definition = await CatalogTaxonomyAccess.GetVisibleEntityAsync(
            _repository,
            attributeDefinitionId,
            _multiTenantFilter,
            CurrentTenant,
            AsyncExecuter);
        await CatalogTaxonomyAccess.EnsureCanMutateTaxonomyEntityAsync(PermissionChecker, definition, CurrentTenant);
        if (definition.GovernanceStatus == AttributeDefinitionGovernanceStatus.Archived)
        {
            throw new BusinessException(CatalogDomainErrorCodes.AttributeDefinitionArchivedMutationBlocked);
        }

        var allowed = AttributeAllowedValues.ParseOrdered(definition.AllowedValuesJson);
        var allowedSet = new HashSet<string>(allowed, StringComparer.OrdinalIgnoreCase);
        var defaultLanguage = await GetDefaultLanguageAsync();

        foreach (var option in input.Options)
        {
            if (string.IsNullOrWhiteSpace(option.Value))
            {
                throw new BusinessException(CatalogDomainErrorCodes.AttributeOptionTranslationUnknownValue)
                    .WithData("Value", option.Value ?? string.Empty);
            }

            var trimmed = option.Value.Trim();
            if (!allowedSet.Contains(trimmed))
            {
                throw new BusinessException(CatalogDomainErrorCodes.AttributeOptionTranslationUnknownValue)
                    .WithData("Value", trimmed);
            }

            var canonical = allowed.First(a => string.Equals(a, trimmed, StringComparison.OrdinalIgnoreCase));
            AttributeOption? optionEntity;
            using (_multiTenantFilter.Disable())
            {
                var oq = await _attributeOptionRepository.GetQueryableAsync();
                optionEntity = await AsyncExecuter.FirstOrDefaultAsync(
                    oq.Where(x =>
                            x.AttributeDefinitionId == attributeDefinitionId &&
                            string.Equals(x.Value, canonical, StringComparison.OrdinalIgnoreCase))
                        .WhereVisibleToTaxonomyReader(CurrentTenant));
            }

            if (optionEntity == null)
            {
                throw new BusinessException(CatalogDomainErrorCodes.AttributeOptionTranslationUnknownValue)
                    .WithData("Value", trimmed);
            }

            var translations = option.Translations ?? new List<AttributeOptionTranslationItemDto>();
            if (translations.Count > 0)
            {
                MultilingualDomainGuard.ValidateRequiredDefaultAndNoDuplicates(
                    translations.Select(x => new TranslationLanguageTag { Language = x.Language }).ToList(),
                    defaultLanguage,
                    CatalogDomainErrorCodes.AttributeOptionTranslationDuplicateLanguage,
                    CatalogDomainErrorCodes.AttributeOptionTranslationDefaultRequired);
            }

            await DeleteOptionTranslationsAsync(optionEntity.Id);

            foreach (var t in translations)
            {
                var tr = new AttributeOptionTranslation(
                    GuidGenerator.Create(),
                    optionEntity.Id,
                    MultilingualDomainGuard.NormalizeLanguage(t.Language),
                    t.DisplayName.Trim());
                tr.TenantId = optionEntity.TenantId;
                await _optionTranslationRepository.InsertAsync(tr);
            }
        }

        return await BuildOptionTranslationDtosAsync(attributeDefinitionId);
    }

    private void ValidateDefinitionTranslations(
        IReadOnlyList<AttributeDefinitionTranslationDto> translations,
        string defaultLanguage)
    {
        MultilingualDomainGuard.ValidateRequiredDefaultAndNoDuplicates(
            translations.Select(x => new TranslationLanguageTag { Language = x.Language }).ToList(),
            defaultLanguage,
            CatalogDomainErrorCodes.AttributeDefinitionDuplicateTranslationLanguage,
            CatalogDomainErrorCodes.AttributeDefinitionDefaultTranslationRequired);
    }

    private async Task ReplaceDefinitionTranslationsAsync(
        Guid definitionId,
        Guid? definitionTenantId,
        IReadOnlyList<AttributeDefinitionTranslationDto> translations)
    {
        using (_multiTenantFilter.Disable())
        {
            var existingQuery = await _translationRepository.GetQueryableAsync();
            var existing = await AsyncExecuter.ToListAsync(existingQuery.Where(x => x.AttributeDefinitionId == definitionId));
            foreach (var e in existing)
            {
                await _translationRepository.DeleteAsync(e);
            }
        }

        foreach (var t in translations)
        {
            var tr = new AttributeDefinitionTranslation(
                GuidGenerator.Create(),
                definitionId,
                MultilingualDomainGuard.NormalizeLanguage(t.Language),
                t.Name,
                t.Description);
            tr.TenantId = definitionTenantId;
            await _translationRepository.InsertAsync(tr);
        }
    }

    private async Task<List<AttributeDefinitionTranslation>> GetDefinitionTranslationsForDefinitionIdsAsync(List<Guid> definitionIds)
    {
        using (_multiTenantFilter.Disable())
        {
            var q = await _translationRepository.GetQueryableAsync();
            return await AsyncExecuter.ToListAsync(q.Where(x => definitionIds.Contains(x.AttributeDefinitionId)));
        }
    }

    private async Task DeleteOptionTranslationsAsync(Guid optionId)
    {
        using (_multiTenantFilter.Disable())
        {
            var q = await _optionTranslationRepository.GetQueryableAsync();
            var rows = await AsyncExecuter.ToListAsync(q.Where(x => x.AttributeOptionId == optionId));
            foreach (var r in rows)
            {
                await _optionTranslationRepository.DeleteAsync(r);
            }
        }
    }

    private async Task<List<AttributeOptionTranslationDto>> BuildOptionTranslationDtosAsync(Guid attributeDefinitionId)
    {
        List<AttributeOption> options;
        using (_multiTenantFilter.Disable())
        {
            var q = await _attributeOptionRepository.GetQueryableAsync();
            options = await AsyncExecuter.ToListAsync(
                q.Where(x => x.AttributeDefinitionId == attributeDefinitionId).WhereVisibleToTaxonomyReader(CurrentTenant));
        }
        var ordered = options
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.CreationTime)
            .ToList();
        if (ordered.Count == 0)
        {
            return new List<AttributeOptionTranslationDto>();
        }

        var optionIds = ordered.Select(x => x.Id).ToList();
        List<AttributeOptionTranslation> stored;
        using (_multiTenantFilter.Disable())
        {
            var tq = await _optionTranslationRepository.GetQueryableAsync();
            stored = await AsyncExecuter.ToListAsync(tq.Where(x => optionIds.Contains(x.AttributeOptionId)));
        }
        var byOptionId = stored
            .GroupBy(x => x.AttributeOptionId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<AttributeOptionTranslation>)g.ToList());

        var defaultLanguage = await GetDefaultLanguageAsync();
        var currentCulture = CultureInfo.CurrentUICulture.Name;

        var result = new List<AttributeOptionTranslationDto>();
        foreach (var option in ordered)
        {
            var list = byOptionId.TryGetValue(option.Id, out var tr) ? tr : Array.Empty<AttributeOptionTranslation>();

            var current = CatalogTranslationResolver.ResolveCurrentCultureOptionTranslation(list, currentCulture);
            var fallback = CatalogTranslationResolver.ResolveFallbackOptionTranslation(list, currentCulture, defaultLanguage);

            result.Add(new AttributeOptionTranslationDto
            {
                Value = option.Value,
                OptionId = option.Id,
                DisplayName = current?.DisplayName,
                DisplayNameLanguage = current?.Language,
                FallbackDisplayName = fallback?.DisplayName,
                FallbackDisplayNameLanguage = fallback?.Language,
                Translations = list
                    .Select(x => new AttributeOptionTranslationItemDto
                    {
                        Language = x.Language,
                        DisplayName = x.DisplayName
                    })
                    .OrderBy(x => x.Language)
                    .ToList()
            });
        }

        return result;
    }

    private async Task SyncAttributeOptionsAsync(AttributeDefinition definition)
    {
        if (definition.DataType != AttributeDefinitionDataType.Enum)
        {
            List<AttributeOption> existingEnumOptions;
            using (_multiTenantFilter.Disable())
            {
                var q = await _attributeOptionRepository.GetQueryableAsync();
                existingEnumOptions = await AsyncExecuter.ToListAsync(
                    q.Where(x => x.AttributeDefinitionId == definition.Id).WhereVisibleToTaxonomyReader(CurrentTenant));
            }

            foreach (var opt in existingEnumOptions)
            {
                await _attributeOptionRepository.DeleteAsync(opt);
            }

            return;
        }

        var ordered = AttributeAllowedValues.ParseOrdered(definition.AllowedValuesJson);
        List<AttributeOption> existing;
        using (_multiTenantFilter.Disable())
        {
            var q = await _attributeOptionRepository.GetQueryableAsync();
            existing = await AsyncExecuter.ToListAsync(
                q.Where(x => x.AttributeDefinitionId == definition.Id).WhereVisibleToTaxonomyReader(CurrentTenant));
        }

        var desired = new HashSet<string>(ordered, StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < ordered.Count; i++)
        {
            var v = ordered[i];
            var match = existing.FirstOrDefault(x => string.Equals(x.Value, v, StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                match.SetDisplayOrder(i);
                await _attributeOptionRepository.UpdateAsync(match);
            }
            else
            {
                var opt = new AttributeOption(GuidGenerator.Create(), definition.Id, v, i);
                opt.TenantId = definition.TenantId;
                await _attributeOptionRepository.InsertAsync(opt);
            }
        }

        foreach (var opt in existing)
        {
            if (!desired.Contains(opt.Value))
            {
                await _attributeOptionRepository.DeleteAsync(opt);
            }
        }
    }

    private async Task<string> GetDefaultLanguageAsync()
    {
        var configured = await _settingProvider.GetOrNullAsync(LocalizationSettingNames.DefaultLanguage);
        return string.IsNullOrWhiteSpace(configured) ? "en" : configured;
    }

    private static AttributeDefinitionDto MapToDto(
        AttributeDefinition entity,
        IReadOnlyList<AttributeDefinitionTranslation> translations,
        string? defaultLanguage)
    {
        var currentCulture = CultureInfo.CurrentUICulture.Name;
        var current = CatalogTranslationResolver.ResolveCurrentCultureDefinitionTranslation(translations, currentCulture);
        var fallback = CatalogTranslationResolver.ResolveFallbackDefinitionTranslation(translations, currentCulture, defaultLanguage);

        return new AttributeDefinitionDto
        {
            Id = entity.Id,
            Key = entity.Key,
            DataType = entity.DataType,
            AllowedValuesJson = entity.AllowedValuesJson,
            RegexPattern = entity.RegexPattern,
            MinValue = entity.MinValue,
            MaxValue = entity.MaxValue,
            IsRequired = entity.IsRequired,
            IsRecommended = entity.IsRecommended,
            GovernanceStatus = entity.GovernanceStatus,
            PublishedVersion = entity.PublishedVersion,
            DisplayName = current?.DisplayName,
            DisplayNameLanguage = current?.Language,
            FallbackDisplayName = fallback?.DisplayName,
            FallbackDisplayNameLanguage = fallback?.Language,
            Description = current?.Description,
            Translations = translations
                .Select(x => new AttributeDefinitionTranslationDto
                {
                    Language = x.Language,
                    Name = x.DisplayName,
                    Description = x.Description
                })
                .OrderBy(x => x.Language)
                .ToList()
        };
    }

    private sealed class TranslationLanguageTag : IEntityTranslation
    {
        public string Language { get; set; } = string.Empty;
    }
}
