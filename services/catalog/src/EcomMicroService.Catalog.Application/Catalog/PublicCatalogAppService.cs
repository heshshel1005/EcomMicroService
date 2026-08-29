using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;

namespace EcomMicroService.Catalog;

/// <summary>
/// Public (storefront) catalog API: category tree, product list/detail with filters and search.
/// </summary>
public class PublicCatalogAppService : CatalogAppService, IPublicCatalogAppService
{
    private readonly IRepository<Category, Guid> _categoryRepository;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<ProductVariant, Guid> _variantRepository;
    private readonly IRepository<ProductVariantAttribute, Guid> _variantAttributeRepository;
    private readonly IRepository<Inventory, Guid> _inventoryRepository;
    private readonly IRepository<ProductAttribute, Guid> _attributeRepository;
    private readonly IRepository<ProductMedia, Guid> _mediaRepository;
    private readonly IRepository<Brand, Guid> _brandRepository;
    private readonly IRepository<BrandModel, Guid> _brandModelRepository;
    private readonly IRepository<ProductTypeAttributeRule, Guid> _productTypeAttributeRuleRepository;
    private readonly IRepository<AttributeDefinition, Guid> _attributeDefinitionRepository;
    private readonly IRepository<AttributeDefinitionTranslation, Guid> _attributeDefinitionTranslationRepository;
    private readonly IRepository<AttributeOption, Guid> _attributeOptionRepository;
    private readonly IRepository<AttributeOptionTranslation, Guid> _attributeOptionTranslationRepository;
    private readonly ISettingProvider _settingProvider;
    private readonly IDataFilter<IMultiTenant> _multiTenantFilter;

    public PublicCatalogAppService(
        IRepository<Category, Guid> categoryRepository,
        IRepository<Product, Guid> productRepository,
        IRepository<ProductVariant, Guid> variantRepository,
        IRepository<ProductVariantAttribute, Guid> variantAttributeRepository,
        IRepository<Inventory, Guid> inventoryRepository,
        IRepository<ProductAttribute, Guid> attributeRepository,
        IRepository<ProductMedia, Guid> mediaRepository,
        IRepository<Brand, Guid> brandRepository,
        IRepository<BrandModel, Guid> brandModelRepository,
        IRepository<ProductTypeAttributeRule, Guid> productTypeAttributeRuleRepository,
        IRepository<AttributeDefinition, Guid> attributeDefinitionRepository,
        IRepository<AttributeDefinitionTranslation, Guid> attributeDefinitionTranslationRepository,
        IRepository<AttributeOption, Guid> attributeOptionRepository,
        IRepository<AttributeOptionTranslation, Guid> attributeOptionTranslationRepository,
        ISettingProvider settingProvider,
        IDataFilter<IMultiTenant> multiTenantFilter)
    {
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
        _variantRepository = variantRepository;
        _variantAttributeRepository = variantAttributeRepository;
        _inventoryRepository = inventoryRepository;
        _attributeRepository = attributeRepository;
        _mediaRepository = mediaRepository;
        _brandRepository = brandRepository;
        _brandModelRepository = brandModelRepository;
        _productTypeAttributeRuleRepository = productTypeAttributeRuleRepository;
        _attributeDefinitionRepository = attributeDefinitionRepository;
        _attributeDefinitionTranslationRepository = attributeDefinitionTranslationRepository;
        _attributeOptionRepository = attributeOptionRepository;
        _attributeOptionTranslationRepository = attributeOptionTranslationRepository;
        _settingProvider = settingProvider;
        _multiTenantFilter = multiTenantFilter;
    }

    [AllowAnonymous]
    public async Task<List<CategoryTreeDto>> GetCategoryTreeAsync()
    {
        var defaultLanguage = await GetDefaultLanguageAsync();
        List<Category> all;
        using (_multiTenantFilter.Disable())
        {
            var q = await _categoryRepository.WithDetailsAsync(x => x.Translations);
            all = await AsyncExecuter.ToListAsync(q.WhereVisibleToTaxonomyReader(CurrentTenant));
        }
        var dtos = all.Select(x => MapCategoryToDto(x, defaultLanguage)).OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name).ToList();
        return BuildCategoryTree(dtos, null);
    }

    [AllowAnonymous]
    public async Task<CatalogFilterOptionsDto> GetFilterOptionsAsync(CatalogFilterOptionsRequestDto input, CancellationToken cancellationToken = default)
    {
        try
        {
            var defaultLanguage = await GetDefaultLanguageAsync();
            var productQuery = await _productRepository.GetQueryableAsync();
            productQuery = productQuery.Where(p => p.IsPublished);
            if (input.CategoryId.HasValue)
            {
                productQuery = productQuery.Where(p => p.CategoryId == input.CategoryId.Value);
            }
            if (input.ProductTypeId.HasValue)
            {
                productQuery = productQuery.Where(p => p.ProductTypeId == input.ProductTypeId.Value);
            }

            var contextProducts = await AsyncExecuter.ToListAsync(productQuery, cancellationToken);
            var publishedBrandIds = contextProducts.Select(p => p.BrandId).Distinct().ToList();
            var publishedModelIds = contextProducts.Where(p => p.ModelId.HasValue).Select(p => p.ModelId!.Value).Distinct().ToList();

            var attributes = await BuildDynamicFilterAttributesAsync(
                contextProducts,
                input.ProductTypeId,
                defaultLanguage,
                cancellationToken);

            var brands = new List<BrandFilterItemDto>();
            if (publishedBrandIds.Count > 0)
            {
                var brandEntities = await AsyncExecuter.ToListAsync(
                    (await _brandRepository.WithDetailsAsync(x => x.Translations))
                    .Where(b => publishedBrandIds.Contains(b.Id) && b.IsActive), cancellationToken);
                brands = brandEntities
                    .Select(b => new BrandFilterItemDto { Id = b.Id, Name = ResolveBrandName(b, defaultLanguage) })
                    .OrderBy(b => b.Name)
                    .ToList();
            }

            var models = new List<ModelFilterItemDto>();
            if (publishedModelIds.Count > 0)
            {
                var modelEntities = await AsyncExecuter.ToListAsync(
                    (await _brandModelRepository.WithDetailsAsync(x => x.Translations))
                    .Where(m => publishedModelIds.Contains(m.Id) && m.IsActive), cancellationToken);
                models = modelEntities
                    .Select(m => new ModelFilterItemDto { Id = m.Id, BrandId = m.BrandId, Name = ResolveBrandModelName(m, defaultLanguage) })
                    .OrderBy(m => m.Name)
                    .ToList();
            }

            return new CatalogFilterOptionsDto
            {
                Attributes = attributes,
                Brands = brands,
                Models = models
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new CatalogFilterOptionsDto();
        }
    }

    private async Task<List<CatalogAttributeFilterItemDto>> BuildDynamicFilterAttributesAsync(
        List<Product> contextProducts,
        Guid? requestedProductTypeId,
        string? defaultLanguage,
        CancellationToken cancellationToken)
    {
        var currentCultureName = CultureInfo.CurrentUICulture.Name;
        var productTypeIds = requestedProductTypeId.HasValue
            ? new List<Guid> { requestedProductTypeId.Value }
            : contextProducts
                .Where(p => p.ProductTypeId.HasValue)
                .Select(p => p.ProductTypeId!.Value)
                .Distinct()
                .ToList();
        if (productTypeIds.Count == 0)
        {
            return new List<CatalogAttributeFilterItemDto>();
        }

        var rules = await CatalogTaxonomyAccess.GetVisibleListAsync(
            _productTypeAttributeRuleRepository,
            _multiTenantFilter,
            CurrentTenant,
            AsyncExecuter,
            x => productTypeIds.Contains(x.ProductTypeId),
            cancellationToken);
        var orderedRules = rules
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.CreationTime)
            .ToList();
        var definitionIds = orderedRules
            .Select(x => x.AttributeDefinitionId)
            .Distinct()
            .ToList();
        if (definitionIds.Count == 0)
        {
            return new List<CatalogAttributeFilterItemDto>();
        }

        var definitions = await CatalogTaxonomyAccess.GetVisibleListAsync(
            _attributeDefinitionRepository,
            _multiTenantFilter,
            CurrentTenant,
            AsyncExecuter,
            x => definitionIds.Contains(x.Id),
            cancellationToken);
        var definitionById = definitions.ToDictionary(x => x.Id);
        var filterableDefinitions = orderedRules
            .Select(r => definitionById.TryGetValue(r.AttributeDefinitionId, out var d) ? d : null)
            .Where(d =>
                d is not null &&
                AttributeDefinitionCatalogGovernance.IsPublishedForCatalog(d) &&
                d.DataType is AttributeDefinitionDataType.Text or AttributeDefinitionDataType.Enum or AttributeDefinitionDataType.Number)
            .DistinctBy(d => d!.Id)
            .Select(d => d!)
            .ToList();
        if (filterableDefinitions.Count == 0)
        {
            return new List<CatalogAttributeFilterItemDto>();
        }

        var filterableDefinitionIds = filterableDefinitions.Select(d => d.Id).Distinct().ToList();
        List<AttributeDefinitionTranslation> definitionTranslations;
        using (_multiTenantFilter.Disable())
        {
            var tq = await _attributeDefinitionTranslationRepository.GetQueryableAsync();
            definitionTranslations = filterableDefinitionIds.Count > 0
                ? await AsyncExecuter.ToListAsync(
                    tq.Where(x => filterableDefinitionIds.Contains(x.AttributeDefinitionId)),
                    cancellationToken)
                : new List<AttributeDefinitionTranslation>();
        }
        var definitionTranslationsByDefinitionId = definitionTranslations
            .GroupBy(x => x.AttributeDefinitionId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<AttributeDefinitionTranslation>)g.ToList());

        List<AttributeOption> attributeOptionsForDefinitions;
        using (_multiTenantFilter.Disable())
        {
            var oq = await _attributeOptionRepository.GetQueryableAsync();
            attributeOptionsForDefinitions = filterableDefinitionIds.Count > 0
                ? await AsyncExecuter.ToListAsync(
                    oq.Where(x => filterableDefinitionIds.Contains(x.AttributeDefinitionId) && x.IsActive)
                        .WhereVisibleToTaxonomyReader(CurrentTenant),
                    cancellationToken)
                : new List<AttributeOption>();
        }
        var optionsByDefinitionId = attributeOptionsForDefinitions
            .GroupBy(x => x.AttributeDefinitionId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<AttributeOption>)g.OrderBy(x => x.DisplayOrder).ThenBy(x => x.CreationTime).ToList());

        var allOptionIds = optionsByDefinitionId.Values.SelectMany(o => o.Select(x => x.Id)).Distinct().ToList();
        List<AttributeOptionTranslation> optionTranslations;
        using (_multiTenantFilter.Disable())
        {
            var otq = await _attributeOptionTranslationRepository.GetQueryableAsync();
            optionTranslations = allOptionIds.Count > 0
                ? await AsyncExecuter.ToListAsync(
                    otq.Where(x => allOptionIds.Contains(x.AttributeOptionId)),
                    cancellationToken)
                : new List<AttributeOptionTranslation>();
        }
        var optionTranslationsByOptionId = optionTranslations
            .GroupBy(x => x.AttributeOptionId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<AttributeOptionTranslation>)g.ToList());

        var valuesByKey = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var definition in filterableDefinitions)
        {
            valuesByKey[definition.Key] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        foreach (var product in contextProducts)
        {
            if (string.IsNullOrWhiteSpace(product.DynamicAttributesJson))
            {
                continue;
            }

            var payload = ParseDynamicAttributes(product.DynamicAttributesJson);
            if (payload.Count == 0)
            {
                continue;
            }

            foreach (var definition in filterableDefinitions)
            {
                if (!payload.TryGetValue(definition.Key, out var value) || string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                valuesByKey[definition.Key].Add(value.Trim());
            }
        }

        var output = new List<CatalogAttributeFilterItemDto>();
        foreach (var definition in filterableDefinitions)
        {
            var values = valuesByKey[definition.Key].OrderBy(x => x).ToList();
            if (values.Count == 0)
            {
                continue;
            }

            definitionTranslationsByDefinitionId.TryGetValue(definition.Id, out var defTranslations);
            var currentDefTranslation = CatalogTranslationResolver.ResolveCurrentCultureDefinitionTranslation(
                defTranslations,
                currentCultureName);
            var fallbackDefTranslation = CatalogTranslationResolver.ResolveFallbackDefinitionTranslation(
                defTranslations,
                currentCultureName,
                defaultLanguage);

            optionsByDefinitionId.TryGetValue(definition.Id, out var optionsForDefinition);

            var localizedValues = new List<CatalogAttributeFilterValueDto>();
            foreach (var invariantValue in values)
            {
                var option = optionsForDefinition?.FirstOrDefault(o =>
                    string.Equals(o.Value, invariantValue, StringComparison.OrdinalIgnoreCase));
                IReadOnlyList<AttributeOptionTranslation>? optionTr = null;
                if (option != null)
                {
                    optionTranslationsByOptionId.TryGetValue(option.Id, out optionTr);
                }

                var currentOpt = CatalogTranslationResolver.ResolveCurrentCultureOptionTranslation(
                    optionTr,
                    currentCultureName);
                var fallbackOpt = CatalogTranslationResolver.ResolveFallbackOptionTranslation(
                    optionTr,
                    currentCultureName,
                    defaultLanguage);

                localizedValues.Add(new CatalogAttributeFilterValueDto
                {
                    Value = invariantValue,
                    DisplayName = currentOpt?.DisplayName,
                    DisplayNameLanguage = currentOpt?.Language,
                    FallbackDisplayName = fallbackOpt?.DisplayName,
                    FallbackDisplayNameLanguage = fallbackOpt?.Language
                });
            }

            output.Add(new CatalogAttributeFilterItemDto
            {
                Key = definition.Key,
                DisplayName = currentDefTranslation?.DisplayName,
                DisplayNameLanguage = currentDefTranslation?.Language,
                FallbackDisplayName = fallbackDefTranslation?.DisplayName,
                FallbackDisplayNameLanguage = fallbackDefTranslation?.Language,
                LocalizedValues = localizedValues,
                Values = values
            });
        }

        return output;
    }

    private static Dictionary<string, string> ParseDynamicAttributes(string? dynamicAttributesJson)
    {
        if (string.IsNullOrWhiteSpace(dynamicAttributesJson))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            using var doc = JsonDocument.Parse(dynamicAttributesJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var property in doc.RootElement.EnumerateObject())
            {
                var value = property.Value.ValueKind switch
                {
                    JsonValueKind.String => property.Value.GetString(),
                    JsonValueKind.Number => property.Value.GetRawText(),
                    JsonValueKind.True => bool.TrueString,
                    JsonValueKind.False => bool.FalseString,
                    _ => null
                };
                if (!string.IsNullOrWhiteSpace(value))
                {
                    result[property.Name] = value!;
                }
            }

            return result;
        }
        catch (JsonException)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static bool MatchesAllDynamicFilters(string? dynamicAttributesJson, IReadOnlyDictionary<string, string> filters)
    {
        if (filters.Count == 0)
        {
            return true;
        }

        var payload = ParseDynamicAttributes(dynamicAttributesJson);
        foreach (var filter in filters)
        {
            if (!payload.TryGetValue(filter.Key, out var value))
            {
                return false;
            }

            if (!string.Equals(value, filter.Value, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    [AllowAnonymous]
    public async Task<PagedResultDto<PublicProductListDto>> GetProductListAsync(PublicProductListRequestDto input, CancellationToken cancellationToken = default)
    {
        try
        {
            var defaultLanguage = await GetDefaultLanguageAsync();
            // Product IDs that match attribute/price filters (null = no filter applied)
            HashSet<Guid>? productIdsFromFilters = null;

            if (input.PriceMin.HasValue || input.PriceMax.HasValue)
            {
                var variantQuery = await _variantRepository.GetQueryableAsync();
                variantQuery = variantQuery.Where(v => v.Price != null);
                if (input.PriceMin.HasValue)
                    variantQuery = variantQuery.Where(v => v.Price >= input.PriceMin.Value);
                if (input.PriceMax.HasValue)
                    variantQuery = variantQuery.Where(v => v.Price <= input.PriceMax.Value);
                var priceProductIds = await AsyncExecuter.ToListAsync(
                    variantQuery.Select(v => v.ProductId).Distinct(), cancellationToken);
                var set = priceProductIds.ToHashSet();
                productIdsFromFilters = productIdsFromFilters == null ? set : new HashSet<Guid>(productIdsFromFilters.Intersect(set));
            }

            var dynamicFilters = ParseDynamicAttributes(input.DynamicFiltersJson);
            if (dynamicFilters.Count > 0)
            {
                var productFilterQuery = await _productRepository.GetQueryableAsync();
                productFilterQuery = productFilterQuery.Where(p => p.IsPublished);
                if (input.CategoryId.HasValue)
                {
                    productFilterQuery = productFilterQuery.Where(p => p.CategoryId == input.CategoryId.Value);
                }
                if (input.ProductTypeId.HasValue)
                {
                    productFilterQuery = productFilterQuery.Where(p => p.ProductTypeId == input.ProductTypeId.Value);
                }
                var filterCandidates = await AsyncExecuter.ToListAsync(productFilterQuery, cancellationToken);
                var dynamicProductIds = filterCandidates
                    .Where(p => MatchesAllDynamicFilters(p.DynamicAttributesJson, dynamicFilters))
                    .Select(p => p.Id)
                    .ToHashSet();
                productIdsFromFilters = productIdsFromFilters == null
                    ? dynamicProductIds
                    : new HashSet<Guid>(productIdsFromFilters.Intersect(dynamicProductIds));
            }

            var query = await _productRepository.WithDetailsAsync(x => x.Translations);
            query = query.Where(p => p.IsPublished);

            if (input.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == input.CategoryId.Value);

            if (input.ProductTypeId.HasValue)
                query = query.Where(p => p.ProductTypeId == input.ProductTypeId.Value);

            if (input.BrandId.HasValue)
                query = query.Where(p => p.BrandId == input.BrandId.Value);

            if (input.ModelId.HasValue)
                query = query.Where(p => p.ModelId == input.ModelId.Value);

            if (!string.IsNullOrWhiteSpace(input.Search))
            {
                var term = input.Search.Trim().ToLowerInvariant();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(term) ||
                    (p.Description != null && p.Description.ToLower().Contains(term)) ||
                    (p.ProductNumber != null && p.ProductNumber.ToLower().Contains(term)));
            }

            if (productIdsFromFilters != null)
            {
                if (productIdsFromFilters.Count == 0)
                    return new PagedResultDto<PublicProductListDto>(0, new List<PublicProductListDto>());
                query = query.Where(p => productIdsFromFilters.Contains(p.Id));
            }

            var total = await AsyncExecuter.CountAsync(query, cancellationToken);

            var sortDesc = input.Sorting?.EndsWith(" DESC", StringComparison.OrdinalIgnoreCase) ?? false;
            var sortKey = (input.Sorting?.Replace(" DESC", "", StringComparison.OrdinalIgnoreCase).Trim()) ?? nameof(Product.Name);
            query = sortKey switch
            {
                nameof(Product.ProductNumber) => sortDesc ? query.OrderByDescending(p => p.ProductNumber) : query.OrderBy(p => p.ProductNumber),
                "PriceFrom" or "Price" => sortDesc ? query.OrderByDescending(p => p.Id) : query.OrderBy(p => p.Id), // resolved below with variant min price
                _ => sortDesc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name)
            };

            var maxResultCount = input.MaxResultCount > 0 ? input.MaxResultCount : 10;
            query = query.Skip(input.SkipCount).Take(maxResultCount);
            var products = await AsyncExecuter.ToListAsync(query, cancellationToken);
            var productIds = products.Select(p => p.Id).ToList();

            var variants = await _variantRepository.GetListAsync(v => productIds.Contains(v.ProductId), cancellationToken: cancellationToken);
            var variantIds = variants.Select(v => v.Id).ToList();
            var inventories = variantIds.Count > 0
                ? await _inventoryRepository.GetListAsync(i => variantIds.Contains(i.ProductVariantId), cancellationToken: cancellationToken)
                : new List<Inventory>();
            var invByVariant = inventories.ToDictionary(i => i.ProductVariantId);
            var pricesByProduct = variants
                .Where(v => v.Price.HasValue)
                .GroupBy(v => v.ProductId)
                .ToDictionary(g => g.Key, g => g.Min(v => v.Price!.Value));
            var productInStock = new HashSet<Guid>();
            foreach (var v in variants)
            {
                if (invByVariant.TryGetValue(v.Id, out var inv) && inv.AvailableQuantity > 0)
                    productInStock.Add(v.ProductId);
            }

            var categoryIds = products.Where(p => p.CategoryId.HasValue).Select(p => p.CategoryId!.Value).Distinct().ToList();
            var categories = categoryIds.Count > 0
                ? await LoadVisibleCategoriesWithDetailsAsync(categoryIds, cancellationToken)
                : new List<Category>();
            var categoryMap = categories.ToDictionary(c => c.Id);

            var brandIds = products.Select(p => p.BrandId).Distinct().ToList();
            var brands = await AsyncExecuter.ToListAsync((await _brandRepository.WithDetailsAsync(x => x.Translations)).Where(b => brandIds.Contains(b.Id)), cancellationToken);
            var brandMap = brands.ToDictionary(b => b.Id);
            var modelIds = products.Where(p => p.ModelId.HasValue).Select(p => p.ModelId!.Value).Distinct().ToList();
            var models = modelIds.Count > 0
                ? await AsyncExecuter.ToListAsync((await _brandModelRepository.WithDetailsAsync(x => x.Translations)).Where(m => modelIds.Contains(m.Id)), cancellationToken)
                : new List<BrandModel>();
            var modelMap = models.ToDictionary(m => m.Id);

            var mediaList = await _mediaRepository.GetListAsync(m =>
                productIds.Contains(m.ProductId) && m.MediaType == ProductMediaType.Image, cancellationToken: cancellationToken);
            var primaryMediaByProduct = mediaList
                .OrderBy(m => m.IsPrimary ? 0 : 1)
                .ThenBy(m => m.SortOrder)
                .GroupBy(m => m.ProductId)
                .ToDictionary(g => g.Key, g => g.First().Id);

            var items = products.Select(p =>
            {
                var dto = new PublicProductListDto
                {
                    Id = p.Id,
                    ProductNumber = p.ProductNumber,
                    Name = ResolveProductName(p, defaultLanguage),
                    CategoryId = p.CategoryId,
                    BrandId = p.BrandId,
                    IsInStock = productInStock.Contains(p.Id)
                };
                if (p.CategoryId.HasValue && categoryMap.TryGetValue(p.CategoryId.Value, out var cat))
                    dto.CategoryName = ResolveCategoryName(cat, defaultLanguage);
                if (brandMap.TryGetValue(p.BrandId, out var brand))
                    dto.BrandName = ResolveBrandName(brand, defaultLanguage);
                if (p.ModelId.HasValue && modelMap.TryGetValue(p.ModelId.Value, out var model))
                    dto.ModelName = ResolveBrandModelName(model, defaultLanguage);
                dto.ModelId = p.ModelId;
                if (pricesByProduct.TryGetValue(p.Id, out var priceFrom))
                    dto.PriceFrom = priceFrom;
                if (primaryMediaByProduct.TryGetValue(p.Id, out var mediaId))
                    dto.PrimaryMediaId = mediaId;
                return dto;
            }).ToList();

            return new PagedResultDto<PublicProductListDto>(total, items);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new PagedResultDto<PublicProductListDto>(0, new List<PublicProductListDto>());
        }
    }

    [AllowAnonymous]
    public async Task<ProductDto> GetProductDetailAsync(Guid id)
    {
        var defaultLanguage = await GetDefaultLanguageAsync();
        var product = await AsyncExecuter.FirstOrDefaultAsync(
            (await _productRepository.WithDetailsAsync(x => x.Translations))
            .Where(p => p.Id == id && p.IsPublished));
        if (product == null)
            throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(Product), id);

        var mediaList = await _mediaRepository.GetListAsync(m => m.ProductId == id);
        var primaryMedia = mediaList
            .Where(m => m.MediaType == ProductMediaType.Image)
            .OrderBy(m => m.IsPrimary ? 0 : 1)
            .ThenBy(m => m.SortOrder)
            .FirstOrDefault();
        var orderedMedia = mediaList.OrderBy(m => m.SortOrder).ToList();
        var allMediaIds = orderedMedia.Select(m => m.Id).ToList();
        var mediaWithType = orderedMedia.Select(m => new ProductMediaItemDto
        {
            Id = m.Id,
            MediaType = m.MediaType
        }).ToList();

        var variants = await _variantRepository.GetListAsync(v => v.ProductId == id);
        var variantIds = variants.Select(v => v.Id).ToList();
        var attributes = variantIds.Count > 0
            ? await _variantAttributeRepository.GetListAsync(a => variantIds.Contains(a.ProductVariantId))
            : new List<ProductVariantAttribute>();
        var inventories = variantIds.Count > 0
            ? await _inventoryRepository.GetListAsync(i => variantIds.Contains(i.ProductVariantId))
            : new List<Inventory>();
        var attributeIds = attributes.Select(a => a.ProductAttributeId).Distinct().ToList();
        var attributeEntities = attributeIds.Count > 0
            ? await _attributeRepository.GetListAsync(a => attributeIds.Contains(a.Id))
            : new List<ProductAttribute>();
        var attributeMap = attributeEntities.ToDictionary(a => a.Id);
        var invMap = inventories.ToDictionary(i => i.ProductVariantId);
        var attrByVariant = attributes.GroupBy(a => a.ProductVariantId).ToDictionary(g => g.Key, g => g.ToList());

        var variantDtos = variants.Select(v =>
        {
            var dto = new ProductVariantDto
            {
                Id = v.Id,
                ProductId = v.ProductId,
                Sku = v.Sku,
                Price = v.Price
            };
            if (invMap.TryGetValue(v.Id, out var inv))
            {
                dto.Quantity = inv.Quantity;
                dto.Reserved = inv.Reserved;
                dto.AvailableQuantity = inv.AvailableQuantity;
            }
            if (attrByVariant.TryGetValue(v.Id, out var attrs))
            {
                dto.Attributes = attrs.Select(a =>
                {
                    var name = attributeMap.TryGetValue(a.ProductAttributeId, out var attr) ? attr.Name : "";
                    return new ProductVariantAttributeDto
                    {
                        ProductAttributeId = a.ProductAttributeId,
                        ProductAttributeName = name,
                        Value = a.Value
                    };
                }).ToList();
            }
            return dto;
        }).OrderBy(v => v.Sku).ToList();

        Brand? brand = null;
        BrandModel? model = null;
        if (product.BrandId != Guid.Empty)
            brand = await AsyncExecuter.FirstOrDefaultAsync(
                (await _brandRepository.WithDetailsAsync(x => x.Translations)).Where(x => x.Id == product.BrandId));
        if (product.ModelId.HasValue)
            model = await AsyncExecuter.FirstOrDefaultAsync(
                (await _brandModelRepository.WithDetailsAsync(x => x.Translations)).Where(x => x.Id == product.ModelId.Value));

        return new ProductDto
        {
            Id = product.Id,
            ProductNumber = product.ProductNumber,
            Name = ResolveProductName(product, defaultLanguage),
            Description = ResolveProductDescription(product, defaultLanguage),
            CategoryId = product.CategoryId,
            BrandId = product.BrandId,
            ModelId = product.ModelId,
            BrandName = brand == null ? string.Empty : ResolveBrandName(brand, defaultLanguage),
            ModelName = model == null ? null : ResolveBrandModelName(model, defaultLanguage),
            IsPublished = product.IsPublished,
            PrimaryMediaId = primaryMedia?.Id,
            MediaIds = allMediaIds,
            Media = mediaWithType,
            Variants = variantDtos
        };
    }

    [AllowAnonymous]
    public async Task<List<ProductDto>> GetCompareAsync(List<Guid> productIds)
    {
        const int maxCompare = 4;
        var ids = productIds?.Where(x => x != Guid.Empty).Distinct().Take(maxCompare).ToList() ?? new List<Guid>();
        if (ids.Count == 0)
            return new List<ProductDto>();

        var products = await _productRepository.GetListAsync(p => ids.Contains(p.Id) && p.IsPublished);
        var foundIds = products.Select(p => p.Id).ToList();
        var result = new List<ProductDto>();
        foreach (var pid in ids)
        {
            if (!foundIds.Contains(pid))
                continue;
            result.Add(await GetProductDetailAsync(pid));
        }
        return result;
    }

    private static List<CategoryTreeDto> BuildCategoryTree(List<CategoryDto> flat, Guid? parentId)
    {
        return flat
            .Where(x => x.ParentId == parentId)
            .Select(dto => new CategoryTreeDto
            {
                Id = dto.Id,
                ParentId = dto.ParentId,
                Name = dto.Name,
                Slug = dto.Slug,
                DisplayOrder = dto.DisplayOrder,
                Children = BuildCategoryTree(flat, dto.Id)
            })
            .ToList();
    }

    private async Task<List<Category>> LoadVisibleCategoriesWithDetailsAsync(
        List<Guid> categoryIds,
        CancellationToken cancellationToken)
    {
        using (_multiTenantFilter.Disable())
        {
            var q = await _categoryRepository.WithDetailsAsync(x => x.Translations);
            return await AsyncExecuter.ToListAsync(
                q.Where(c => categoryIds.Contains(c.Id)).WhereVisibleToTaxonomyReader(CurrentTenant),
                cancellationToken);
        }
    }

    private async Task<string> GetDefaultLanguageAsync()
    {
        var configured = await _settingProvider.GetOrNullAsync(LocalizationSettingNames.DefaultLanguage);
        return string.IsNullOrWhiteSpace(configured) ? "en" : configured;
    }

    private static CategoryDto MapCategoryToDto(Category entity, string? defaultLanguage)
    {
        var resolved = CatalogTranslationResolver.Resolve(entity.Translations, CultureInfo.CurrentUICulture.Name, defaultLanguage);
        return new CategoryDto
        {
            Id = entity.Id,
            ParentId = entity.ParentId,
            Name = resolved?.Name ?? entity.Name,
            Slug = entity.Slug,
            DisplayOrder = entity.DisplayOrder
        };
    }

    private static string ResolveProductName(Product product, string? defaultLanguage)
    {
        var resolved = CatalogTranslationResolver.Resolve(product.Translations, CultureInfo.CurrentUICulture.Name, defaultLanguage);
        return resolved?.Name ?? product.Name;
    }

    private static string? ResolveProductDescription(Product product, string? defaultLanguage)
    {
        var resolved = CatalogTranslationResolver.Resolve(product.Translations, CultureInfo.CurrentUICulture.Name, defaultLanguage);
        return resolved?.Description ?? product.Description;
    }

    private static string ResolveCategoryName(Category category, string? defaultLanguage)
    {
        var resolved = CatalogTranslationResolver.Resolve(category.Translations, CultureInfo.CurrentUICulture.Name, defaultLanguage);
        return resolved?.Name ?? category.Name;
    }

    private static string ResolveBrandName(Brand brand, string? defaultLanguage)
    {
        var resolved = CatalogTranslationResolver.Resolve(brand.Translations, CultureInfo.CurrentUICulture.Name, defaultLanguage);
        return resolved?.Name ?? brand.Name;
    }

    private static string ResolveBrandModelName(BrandModel model, string? defaultLanguage)
    {
        var resolved = CatalogTranslationResolver.Resolve(model.Translations, CultureInfo.CurrentUICulture.Name, defaultLanguage);
        return resolved?.Name ?? model.Name;
    }

    [AllowAnonymous]
    public async Task<List<VariantCartInfoDto>> GetVariantCartInfoAsync(List<Guid> variantIds)
    {
        var result = new List<VariantCartInfoDto>();
        if (variantIds == null || variantIds.Count == 0)
        {
            return result;
        }

        var ids = variantIds.Distinct().ToList();
        var variants = await _variantRepository.GetListAsync(v => ids.Contains(v.Id));
        var productIds = variants.Select(v => v.ProductId).Distinct().ToList();
        var products = await _productRepository.GetListAsync(p => productIds.Contains(p.Id));
        var inventories = await _inventoryRepository.GetListAsync(i => ids.Contains(i.ProductVariantId));
        var productMap = products.ToDictionary(p => p.Id);
        var invMap = inventories.ToDictionary(i => i.ProductVariantId);

        foreach (var variant in variants)
        {
            productMap.TryGetValue(variant.ProductId, out var product);
            invMap.TryGetValue(variant.Id, out var inv);
            result.Add(new VariantCartInfoDto
            {
                Id = variant.Id,
                ProductId = variant.ProductId,
                ProductName = product?.Name ?? string.Empty,
                Sku = variant.Sku,
                UnitPrice = variant.Price,
                AvailableQuantity = inv?.AvailableQuantity ?? 0
            });
        }

        return result;
    }
}
