using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EcomMicroService.Catalog.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Data;
using Volo.Abp.Localization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;

namespace EcomMicroService.Catalog;

public class ProductAppService : CatalogAppService, IProductAppService
{
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<ProductVariant, Guid> _variantRepository;
    private readonly IRepository<ProductVariantAttribute, Guid> _variantAttributeRepository;
    private readonly IRepository<Inventory, Guid> _inventoryRepository;
    private readonly IRepository<Category, Guid> _categoryRepository;
    private readonly IRepository<Brand, Guid> _brandRepository;
    private readonly IRepository<BrandModel, Guid> _brandModelRepository;
    private readonly IRepository<ProductAttribute, Guid> _attributeRepository;
    private readonly IRepository<ProductType, Guid> _productTypeRepository;
    private readonly IRepository<ProductTypeAttributeRule, Guid> _productTypeAttributeRuleRepository;
    private readonly IRepository<AttributeDefinition, Guid> _attributeDefinitionRepository;
    private readonly IRepository<AttributeDefinitionTranslation, Guid> _attributeDefinitionTranslationRepository;
    private readonly IRepository<AttributeOption, Guid> _attributeOptionRepository;
    private readonly IRepository<AttributeOptionTranslation, Guid> _attributeOptionTranslationRepository;
    private readonly IRepository<ProductMedia, Guid> _mediaRepository;
    private readonly IProductMediaAppService _productMediaAppService;
    private readonly ISettingProvider _settingProvider;
    private readonly IDataFilter<IMultiTenant> _multiTenantFilter;

    public ProductAppService(
        IRepository<Product, Guid> productRepository,
        IRepository<ProductVariant, Guid> variantRepository,
        IRepository<ProductVariantAttribute, Guid> variantAttributeRepository,
        IRepository<Inventory, Guid> inventoryRepository,
        IRepository<Category, Guid> categoryRepository,
        IRepository<Brand, Guid> brandRepository,
        IRepository<BrandModel, Guid> brandModelRepository,
        IRepository<ProductAttribute, Guid> attributeRepository,
        IRepository<ProductType, Guid> productTypeRepository,
        IRepository<ProductTypeAttributeRule, Guid> productTypeAttributeRuleRepository,
        IRepository<AttributeDefinition, Guid> attributeDefinitionRepository,
        IRepository<AttributeDefinitionTranslation, Guid> attributeDefinitionTranslationRepository,
        IRepository<AttributeOption, Guid> attributeOptionRepository,
        IRepository<AttributeOptionTranslation, Guid> attributeOptionTranslationRepository,
        IRepository<ProductMedia, Guid> mediaRepository,
        IProductMediaAppService productMediaAppService,
        ISettingProvider settingProvider,
        IDataFilter<IMultiTenant> multiTenantFilter)
    {
        _productRepository = productRepository;
        _variantRepository = variantRepository;
        _variantAttributeRepository = variantAttributeRepository;
        _inventoryRepository = inventoryRepository;
        _categoryRepository = categoryRepository;
        _brandRepository = brandRepository;
        _brandModelRepository = brandModelRepository;
        _attributeRepository = attributeRepository;
        _productTypeRepository = productTypeRepository;
        _productTypeAttributeRuleRepository = productTypeAttributeRuleRepository;
        _attributeDefinitionRepository = attributeDefinitionRepository;
        _attributeDefinitionTranslationRepository = attributeDefinitionTranslationRepository;
        _attributeOptionRepository = attributeOptionRepository;
        _attributeOptionTranslationRepository = attributeOptionTranslationRepository;
        _mediaRepository = mediaRepository;
        _productMediaAppService = productMediaAppService;
        _settingProvider = settingProvider;
        _multiTenantFilter = multiTenantFilter;
    }

    [Authorize(CatalogPermissions.Catalog.Default)]
    public async Task<PagedResultDto<ProductListDto>> GetListAsync(ProductListRequestDto input)
    {
        var defaultLanguage = await GetDefaultLanguageAsync();
        var query = await _productRepository.WithDetailsAsync(x => x.Translations);
        if (input.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == input.CategoryId.Value);
        if (input.IsPublished.HasValue)
            query = query.Where(p => p.IsPublished == input.IsPublished.Value);
        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var f = input.Filter.Trim().ToLowerInvariant();
            query = query.Where(p =>
                p.Name.ToLower().Contains(f) ||
                (p.ProductNumber != null && p.ProductNumber.ToLower().Contains(f)));
        }

        var total = await AsyncExecuter.CountAsync(query);

        var sortDesc = input.Sorting?.EndsWith(" DESC", StringComparison.OrdinalIgnoreCase) ?? false;
        var sortKey = (input.Sorting?.Replace(" DESC", "", StringComparison.OrdinalIgnoreCase).Trim()) ?? nameof(Product.Name);
        query = sortKey switch
        {
            nameof(Product.ProductNumber) => sortDesc ? query.OrderByDescending(p => p.ProductNumber) : query.OrderBy(p => p.ProductNumber),
            _ => sortDesc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name)
        };

        var maxResultCount = input.MaxResultCount > 0 ? input.MaxResultCount : 10;
        query = query.Skip(input.SkipCount).Take(maxResultCount);
        var products = await AsyncExecuter.ToListAsync(query);
        var productIds = products.Select(p => p.Id).ToList();

        var variants = await _variantRepository.GetListAsync(v => productIds.Contains(v.ProductId));
        var variantIds = variants.Select(v => v.Id).ToList();
        var inventories = variantIds.Count > 0
            ? await _inventoryRepository.GetListAsync(i => variantIds.Contains(i.ProductVariantId))
            : new List<Inventory>();
        var invByVariant = inventories.ToDictionary(i => i.ProductVariantId);
        var pricesByProduct = variants
            .Where(v => v.Price.HasValue)
            .GroupBy(v => v.ProductId)
            .ToDictionary(g => g.Key, g => g.Min(v => v.Price!.Value));

        var categoryIds = products.Where(p => p.CategoryId.HasValue).Select(p => p.CategoryId!.Value).Distinct().ToList();
        var categories = categoryIds.Count > 0
            ? await LoadVisibleCategoriesWithDetailsAsync(categoryIds)
            : new List<Category>();
        var categoryMap = categories.ToDictionary(c => c.Id);

        var brandIds = products.Select(p => p.BrandId).Distinct().ToList();
        var brands = brandIds.Count > 0
            ? await AsyncExecuter.ToListAsync((await _brandRepository.WithDetailsAsync(x => x.Translations)).Where(b => brandIds.Contains(b.Id)))
            : new List<Brand>();
        var brandMap = brands.ToDictionary(b => b.Id);

        var modelIds = products.Where(p => p.ModelId.HasValue).Select(p => p.ModelId!.Value).Distinct().ToList();
        var models = modelIds.Count > 0
            ? await AsyncExecuter.ToListAsync((await _brandModelRepository.WithDetailsAsync(x => x.Translations)).Where(m => modelIds.Contains(m.Id)))
            : new List<BrandModel>();
        var modelMap = models.ToDictionary(m => m.Id);

        var productTypeIds = products.Where(p => p.ProductTypeId.HasValue).Select(p => p.ProductTypeId!.Value).Distinct().ToList();
        var productTypeMap = productTypeIds.Count > 0
            ? (await LoadVisibleProductTypesWithDetailsAsync(productTypeIds)).ToDictionary(x => x.Id)
            : new Dictionary<Guid, ProductType>();

        var rulesByProductType = new Dictionary<Guid, List<ProductTypeAttributeRule>>();
        if (productTypeIds.Count > 0)
        {
            var allRules = await CatalogTaxonomyAccess.GetVisibleListAsync(
                _productTypeAttributeRuleRepository,
                _multiTenantFilter,
                CurrentTenant,
                AsyncExecuter,
                x => productTypeIds.Contains(x.ProductTypeId));
            rulesByProductType = allRules
                .GroupBy(x => x.ProductTypeId)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        var definitionIds = rulesByProductType
            .SelectMany(x => x.Value)
            .Select(x => x.AttributeDefinitionId)
            .Distinct()
            .ToList();
        var definitionsById = definitionIds.Count > 0
            ? (await CatalogTaxonomyAccess.GetVisibleListAsync(
                _attributeDefinitionRepository,
                _multiTenantFilter,
                CurrentTenant,
                AsyncExecuter,
                x => definitionIds.Contains(x.Id))).ToDictionary(x => x.Id)
            : new Dictionary<Guid, AttributeDefinition>();

        var items = products.Select(p =>
        {
            var dto = new ProductListDto
            {
                Id = p.Id,
                ProductNumber = p.ProductNumber,
                Name = ResolveProductName(p, defaultLanguage),
                CategoryId = p.CategoryId,
                BrandId = p.BrandId,
                ModelId = p.ModelId,
                ProductTypeId = p.ProductTypeId,
                IsPublished = p.IsPublished
            };
            if (p.CategoryId.HasValue && categoryMap.TryGetValue(p.CategoryId.Value, out var cat))
                dto.CategoryName = ResolveCategoryName(cat, defaultLanguage);
            if (brandMap.TryGetValue(p.BrandId, out var brand))
                dto.BrandName = ResolveBrandName(brand, defaultLanguage);
            if (p.ModelId.HasValue && modelMap.TryGetValue(p.ModelId.Value, out var model))
                dto.ModelName = ResolveBrandModelName(model, defaultLanguage);
            if (p.ProductTypeId.HasValue && productTypeMap.TryGetValue(p.ProductTypeId.Value, out var productType))
                dto.ProductTypeName = ResolveProductTypeName(productType, defaultLanguage);
            if (pricesByProduct.TryGetValue(p.Id, out var priceFrom))
                dto.PriceFrom = priceFrom;

            if (p.ProductTypeId.HasValue &&
                rulesByProductType.TryGetValue(p.ProductTypeId.Value, out var typeRules) &&
                typeRules.Count > 0)
            {
                var requiredKeys = typeRules
                    .Where(x =>
                        definitionsById.TryGetValue(x.AttributeDefinitionId, out var definition) &&
                        definition.IsRequired &&
                        AttributeDefinitionCatalogGovernance.IsPublishedForCatalog(definition))
                    .Select(x => definitionsById[x.AttributeDefinitionId].Key)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var dynamicAttributes = ParseDynamicAttributes(p.DynamicAttributesJson);
                var filledRequiredCount = requiredKeys.Count(key => IsFilledAttributeValue(dynamicAttributes, key));

                dto.RequiredAttributeCount = requiredKeys.Count;
                dto.FilledRequiredAttributeCount = filledRequiredCount;
                dto.IsAttributeComplete = requiredKeys.Count == 0 || filledRequiredCount == requiredKeys.Count;
            }
            else
            {
                dto.RequiredAttributeCount = 0;
                dto.FilledRequiredAttributeCount = 0;
                dto.IsAttributeComplete = !p.ProductTypeId.HasValue;
            }
            return dto;
        }).ToList();

        return new PagedResultDto<ProductListDto>(total, items);
    }

    private static Dictionary<string, object?> ParseDynamicAttributes(string? dynamicAttributesJson)
    {
        if (string.IsNullOrWhiteSpace(dynamicAttributesJson))
        {
            return new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            var values = JsonSerializer.Deserialize<Dictionary<string, object?>>(dynamicAttributesJson);
            return values == null
                ? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, object?>(values, StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static bool IsFilledAttributeValue(Dictionary<string, object?> dynamicAttributes, string key)
    {
        if (!dynamicAttributes.TryGetValue(key, out var value) || value is null)
        {
            return false;
        }

        if (value is string stringValue)
        {
            return !string.IsNullOrWhiteSpace(stringValue);
        }

        if (value is JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Undefined => false,
                JsonValueKind.Null => false,
                JsonValueKind.String => !string.IsNullOrWhiteSpace(element.GetString()),
                JsonValueKind.Array => element.GetArrayLength() > 0,
                JsonValueKind.Object => true,
                _ => true
            };
        }

        return true;
    }

    [Authorize(CatalogPermissions.Catalog.Default)]
    public async Task<ProductDto> GetAsync(Guid id)
    {
        var defaultLanguage = await GetDefaultLanguageAsync();
        var productQuery = await _productRepository.WithDetailsAsync(x => x.Translations);
        var product = await AsyncExecuter.FirstAsync(productQuery.Where(x => x.Id == id));
        var brandQuery = await _brandRepository.WithDetailsAsync(x => x.Translations);
        var brand = await AsyncExecuter.FirstAsync(brandQuery.Where(x => x.Id == product.BrandId));
        BrandModel? model = product.ModelId.HasValue
            ? await AsyncExecuter.FirstAsync((await _brandModelRepository.WithDetailsAsync(x => x.Translations)).Where(x => x.Id == product.ModelId.Value))
            : null;
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
                Price = v.Price,
                DynamicAttributes = ParseDynamicAttributes(v.DynamicAttributesJson)
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

        return new ProductDto
        {
            Id = product.Id,
            ProductNumber = product.ProductNumber,
            Name = ResolveProductName(product, defaultLanguage),
            Description = ResolveProductDescription(product, defaultLanguage),
            CategoryId = product.CategoryId,
            BrandId = product.BrandId,
            ModelId = product.ModelId,
            ProductTypeId = product.ProductTypeId,
            BrandName = ResolveBrandName(brand, defaultLanguage),
            ModelName = model == null ? null : ResolveBrandModelName(model, defaultLanguage),
            DynamicAttributes = ParseDynamicAttributes(product.DynamicAttributesJson),
            IsPublished = product.IsPublished,
            Variants = variantDtos,
            Translations = product.Translations
                .Select(x => new ProductTranslationDto
                {
                    Language = x.Language,
                    Name = x.Name,
                    Description = x.Description
                })
                .OrderBy(x => x.Language)
                .ToList()
        };
    }

    [Authorize(CatalogPermissions.Catalog.Default)]
    public async Task<List<ProductAttributeDto>> GetAttributesAsync()
    {
        var list = await _attributeRepository.GetListAsync();
        return list.OrderBy(a => a.Name).Select(a => new ProductAttributeDto { Id = a.Id, Name = a.Name }).ToList();
    }

    [Authorize(CatalogPermissions.Catalog.Default)]
    public async Task<List<ProductTypeDto>> GetProductTypesAsync()
    {
        var defaultLanguage = await GetDefaultLanguageAsync();
        using (_multiTenantFilter.Disable())
        {
            var query = await _productTypeRepository.WithDetailsAsync(x => x.Translations);
            var productTypes = await AsyncExecuter.ToListAsync(query.WhereVisibleToTaxonomyReader(CurrentTenant));
            return productTypes
                .Select(x => MapProductTypeToDto(x, defaultLanguage))
                .OrderBy(x => x.Name)
                .ToList();
        }
    }

    [Authorize(CatalogPermissions.Catalog.Default)]
    public async Task<ProductTypeAttributeRequirementsDto> GetAttributeRequirementsByProductTypeAsync(Guid productTypeId)
    {
        var defaultLanguage = await GetDefaultLanguageAsync();
        var currentCultureName = CultureInfo.CurrentUICulture.Name;
        var productType = await CatalogTaxonomyAccess.GetVisibleEntityAsync(
            _productTypeRepository,
            productTypeId,
            _multiTenantFilter,
            CurrentTenant,
            AsyncExecuter);

        var rules = await CatalogTaxonomyAccess.GetVisibleListAsync(
            _productTypeAttributeRuleRepository,
            _multiTenantFilter,
            CurrentTenant,
            AsyncExecuter,
            x => x.ProductTypeId == productTypeId && x.TenantId == productType.TenantId);
        var orderedRules = rules
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.CreationTime)
            .ToList();

        var definitionIds = orderedRules
            .Select(x => x.AttributeDefinitionId)
            .Distinct()
            .ToList();

        var definitions = definitionIds.Count > 0
            ? await CatalogTaxonomyAccess.GetVisibleListAsync(
                _attributeDefinitionRepository,
                _multiTenantFilter,
                CurrentTenant,
                AsyncExecuter,
                x => definitionIds.Contains(x.Id))
            : new List<AttributeDefinition>();
        var definitionById = definitions.ToDictionary(x => x.Id);
        List<AttributeDefinitionTranslation> definitionTranslations;
        using (_multiTenantFilter.Disable())
        {
            var tq = await _attributeDefinitionTranslationRepository.GetQueryableAsync();
            definitionTranslations = definitionIds.Count > 0
                ? await AsyncExecuter.ToListAsync(tq.Where(x => definitionIds.Contains(x.AttributeDefinitionId)))
                : new List<AttributeDefinitionTranslation>();
        }

        var definitionTranslationsByDefinitionId = definitionTranslations
            .GroupBy(x => x.AttributeDefinitionId)
            .ToDictionary(g => g.Key, g => g.ToList());

        List<AttributeOption> attributeOptionsForDefinitions;
        using (_multiTenantFilter.Disable())
        {
            var oq = await _attributeOptionRepository.GetQueryableAsync();
            attributeOptionsForDefinitions = definitionIds.Count > 0
                ? await AsyncExecuter.ToListAsync(
                    oq.Where(x => definitionIds.Contains(x.AttributeDefinitionId)).WhereVisibleToTaxonomyReader(CurrentTenant))
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
                ? await AsyncExecuter.ToListAsync(otq.Where(x => allOptionIds.Contains(x.AttributeOptionId)))
                : new List<AttributeOptionTranslation>();
        }
        var optionTranslationsByOptionId = optionTranslations
            .GroupBy(x => x.AttributeOptionId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var output = new ProductTypeAttributeRequirementsDto
        {
            ProductTypeId = productTypeId
        };

        foreach (var rule in orderedRules)
        {
            if (!definitionById.TryGetValue(rule.AttributeDefinitionId, out var definition))
            {
                continue;
            }

            if (!AttributeDefinitionCatalogGovernance.IsPublishedForCatalog(definition))
            {
                continue;
            }

            var item = new ProductTypeAttributeRequirementItemDto
            {
                AttributeDefinitionId = definition.Id,
                Key = definition.Key,
                DataType = definition.DataType,
                AllowedValuesJson = definition.AllowedValuesJson,
                RegexPattern = definition.RegexPattern,
                MinValue = definition.MinValue,
                MaxValue = definition.MaxValue,
                DisplayOrder = rule.DisplayOrder,
                IsRequired = definition.IsRequired,
                IsRecommended = definition.IsRecommended,
                ConditionalAttributeKey = rule.ConditionalAttributeKey,
                ConditionalOperator = rule.ConditionalOperator,
                ConditionalExpectedValue = rule.ConditionalExpectedValue
            };

            if (definitionTranslationsByDefinitionId.TryGetValue(definition.Id, out var localizedDefinitionTranslations))
            {
                var currentTranslation = CatalogTranslationResolver.ResolveCurrentCultureDefinitionTranslation(
                    localizedDefinitionTranslations,
                    currentCultureName);
                var fallbackTranslation = CatalogTranslationResolver.ResolveFallbackDefinitionTranslation(
                    localizedDefinitionTranslations,
                    currentCultureName,
                    defaultLanguage);

                item.DisplayName = currentTranslation?.DisplayName;
                item.DisplayNameLanguage = currentTranslation?.Language;
                item.FallbackDisplayName = fallbackTranslation?.DisplayName;
                item.FallbackDisplayNameLanguage = fallbackTranslation?.Language;
                item.Description = currentTranslation?.Description;
                item.DescriptionLanguage = currentTranslation?.Language;
                item.FallbackDescription = fallbackTranslation?.Description;
                item.FallbackDescriptionLanguage = fallbackTranslation?.Language;
            }

            if (optionsByDefinitionId.TryGetValue(definition.Id, out var attributeOptionsForDefinition))
            {
                item.LocalizedOptions = attributeOptionsForDefinition
                    .Select(option =>
                    {
                        optionTranslationsByOptionId.TryGetValue(option.Id, out var localizedOptionTranslations);
                        var currentOptionTranslation = CatalogTranslationResolver.ResolveCurrentCultureOptionTranslation(
                            localizedOptionTranslations,
                            currentCultureName);
                        var fallbackOptionTranslation = CatalogTranslationResolver.ResolveFallbackOptionTranslation(
                            localizedOptionTranslations,
                            currentCultureName,
                            defaultLanguage);
                        return new ProductTypeAttributeOptionDto
                        {
                            Value = option.Value,
                            DisplayName = currentOptionTranslation?.DisplayName,
                            DisplayNameLanguage = currentOptionTranslation?.Language,
                            FallbackDisplayName = fallbackOptionTranslation?.DisplayName,
                            FallbackDisplayNameLanguage = fallbackOptionTranslation?.Language
                        };
                    })
                    .ToList();
            }

            var isConditional = !string.IsNullOrWhiteSpace(rule.ConditionalAttributeKey) || rule.ConditionalOperator.HasValue;
            if (isConditional)
            {
                output.ConditionalAttributes.Add(item);
            }
            else if (definition.IsRequired)
            {
                output.RequiredAttributes.Add(item);
            }
            else if (definition.IsRecommended)
            {
                output.RecommendedAttributes.Add(item);
            }
        }

        return output;
    }

    [Authorize(CatalogPermissions.Catalog.Default)]
    public async Task<ProductDto> CreateAsync(CreateProductDto input)
    {
        await ValidateProductNumberAsync(input.ProductNumber, null);
        await ValidateDynamicAttributesAsync(input.ProductTypeId, input.DynamicAttributes);
        foreach (var variantInput in input.Variants)
        {
            await ValidateDynamicAttributesAsync(input.ProductTypeId, variantInput.DynamicAttributes);
        }
        if (input.CategoryId.HasValue)
        {
            await CatalogTaxonomyAccess.GetVisibleEntityAsync(
                _categoryRepository,
                input.CategoryId.Value,
                _multiTenantFilter,
                CurrentTenant,
                AsyncExecuter);
        }

        await _brandRepository.GetAsync(input.BrandId);
        if (input.ModelId.HasValue)
        {
            var model = await _brandModelRepository.GetAsync(input.ModelId.Value);
            if (model.BrandId != input.BrandId)
            {
                throw new Volo.Abp.BusinessException("ECommerce:ModelMustBelongToBrand");
            }
        }

        var productId = GuidGenerator.Create();
        var product = new Product(
            productId,
            input.ProductNumber,
            input.Name,
            input.BrandId,
            input.Description,
            input.CategoryId,
            input.ModelId,
            input.IsPublished,
            input.ProductTypeId,
            SerializeDynamicAttributes(input.DynamicAttributes));
        var defaultLanguage = await GetDefaultLanguageAsync();
        product.SetTranslations(
            input.Translations.Select(x => new ProductTranslation(
                GuidGenerator.Create(),
                productId,
                x.Language,
                x.Name,
                x.Description)),
            defaultLanguage);
        await _productRepository.InsertAsync(product, autoSave: true);

        var attributeIds = input.Variants
            .SelectMany(v => v.Attributes.Select(a => a.ProductAttributeId))
            .Distinct()
            .ToList();
        var attributeEntities = attributeIds.Count > 0
            ? await _attributeRepository.GetListAsync(a => attributeIds.Contains(a.Id))
            : new List<ProductAttribute>();
        var attributeMap = attributeEntities.ToDictionary(a => a.Id);

        var variantDtos = new List<ProductVariantDto>();
        foreach (var v in input.Variants)
        {
            var dto = await CreateVariantWithInventoryAsync(productId, v.Sku, v.Price, v.Quantity, v.DynamicAttributes, v.Attributes, attributeMap);
            if (dto != null)
                variantDtos.Add(dto);
        }

        _ = variantDtos.OrderBy(x => x.Sku).ToList();
        return await GetAsync(product.Id);
    }

    [Authorize(CatalogPermissions.Catalog.Default)]
    public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto input)
    {
        var productQuery = await _productRepository.WithDetailsAsync(x => x.Translations);
        var product = await AsyncExecuter.FirstAsync(productQuery.Where(x => x.Id == id));
        await ValidateProductNumberAsync(input.ProductNumber, id);
        await ValidateDynamicAttributesAsync(input.ProductTypeId, input.DynamicAttributes);
        foreach (var variantInput in input.Variants)
        {
            await ValidateDynamicAttributesAsync(input.ProductTypeId, variantInput.DynamicAttributes);
        }
        if (input.CategoryId.HasValue)
        {
            await CatalogTaxonomyAccess.GetVisibleEntityAsync(
                _categoryRepository,
                input.CategoryId.Value,
                _multiTenantFilter,
                CurrentTenant,
                AsyncExecuter);
        }

        var brand = await _brandRepository.GetAsync(input.BrandId);
        BrandModel? model = null;
        if (input.ModelId.HasValue)
        {
            model = await _brandModelRepository.GetAsync(input.ModelId.Value);
            if (model.BrandId != input.BrandId)
            {
                throw new Volo.Abp.BusinessException("ECommerce:ModelMustBelongToBrand");
            }
        }

        product.SetBrandAndModel(input.BrandId, input.ModelId, model?.BrandId);
        product.ProductNumber = input.ProductNumber;
        product.Name = input.Name;
        product.Description = input.Description;
        product.CategoryId = input.CategoryId;
        product.ProductTypeId = input.ProductTypeId;
        product.DynamicAttributesJson = SerializeDynamicAttributes(input.DynamicAttributes);
        product.IsPublished = input.IsPublished;
        var defaultLanguage = await GetDefaultLanguageAsync();
        product.SetTranslations(
            input.Translations.Select(x => new ProductTranslation(
                GuidGenerator.Create(),
                id,
                x.Language,
                x.Name,
                x.Description)),
            defaultLanguage);
        await _productRepository.UpdateAsync(product);

        var existingVariants = await _variantRepository.GetListAsync(v => v.ProductId == id);
        var existingIds = existingVariants.Select(v => v.Id).ToHashSet();
        var inputIds = input.Variants.Where(v => v.Id.HasValue).Select(v => v.Id!.Value).ToHashSet();
        var toDelete = existingIds.Except(inputIds).ToList();

        foreach (var variantId in toDelete)
        {
            var inv = await _inventoryRepository.FirstOrDefaultAsync(i => i.ProductVariantId == variantId);
            if (inv != null)
                await _inventoryRepository.DeleteAsync(inv);
            var attrs = await _variantAttributeRepository.GetListAsync(a => a.ProductVariantId == variantId);
            foreach (var a in attrs)
                await _variantAttributeRepository.DeleteAsync(a);
            await _variantRepository.DeleteAsync(variantId);
        }

        foreach (var v in input.Variants)
        {
            if (v.Id.HasValue && existingIds.Contains(v.Id.Value))
            {
                var variant = await _variantRepository.GetAsync(v.Id.Value);
                variant.Sku = v.Sku;
                variant.Price = v.Price;
                variant.DynamicAttributesJson = SerializeDynamicAttributes(v.DynamicAttributes);
                await _variantRepository.UpdateAsync(variant);

                var inv = await _inventoryRepository.FirstOrDefaultAsync(i => i.ProductVariantId == variant.Id);
                if (inv != null)
                {
                    inv.Quantity = v.Quantity;
                    await _inventoryRepository.UpdateAsync(inv);
                }
                else
                {
                    var newInv = new Inventory(GuidGenerator.Create(), variant.Id, v.Quantity, 0);
                    await _inventoryRepository.InsertAsync(newInv);
                }

                var existingAttrs = await _variantAttributeRepository.GetListAsync(a => a.ProductVariantId == variant.Id);
                foreach (var a in existingAttrs)
                    await _variantAttributeRepository.DeleteAsync(a);
                foreach (var attr in v.Attributes)
                {
                    var attrId = GuidGenerator.Create();
                    var va = new ProductVariantAttribute(attrId, variant.Id, attr.ProductAttributeId, attr.Value);
                    await _variantAttributeRepository.InsertAsync(va);
                }
            }
            else
            {
                await CreateVariantWithInventoryAsync(id, v.Sku, v.Price, v.Quantity, v.DynamicAttributes, v.Attributes, null);
            }
        }

        return await GetAsync(id);
    }

    private static string ResolveProductName(Product product, string? defaultLanguage)
    {
        var resolved = CatalogTranslationResolver.Resolve(
            product.Translations,
            CultureInfo.CurrentUICulture.Name,
            defaultLanguage);
        return resolved?.Name ?? product.Name;
    }

    private static string? ResolveProductDescription(Product product, string? defaultLanguage)
    {
        var resolved = CatalogTranslationResolver.Resolve(
            product.Translations,
            CultureInfo.CurrentUICulture.Name,
            defaultLanguage);
        return resolved?.Description ?? product.Description;
    }

    private static string ResolveCategoryName(Category category, string? defaultLanguage)
    {
        var resolved = CatalogTranslationResolver.Resolve(
            category.Translations,
            CultureInfo.CurrentUICulture.Name,
            defaultLanguage);
        return resolved?.Name ?? category.Name;
    }

    private static string ResolveBrandName(Brand brand, string? defaultLanguage)
    {
        var resolved = CatalogTranslationResolver.Resolve(
            brand.Translations,
            CultureInfo.CurrentUICulture.Name,
            defaultLanguage);
        return resolved?.Name ?? brand.Name;
    }

    private static string ResolveBrandModelName(BrandModel model, string? defaultLanguage)
    {
        var resolved = CatalogTranslationResolver.Resolve(
            model.Translations,
            CultureInfo.CurrentUICulture.Name,
            defaultLanguage);
        return resolved?.Name ?? model.Name;
    }

    private static string ResolveProductTypeName(ProductType productType, string? defaultLanguage)
    {
        var resolved = CatalogTranslationResolver.Resolve(
            productType.Translations,
            CultureInfo.CurrentUICulture.Name,
            defaultLanguage);
        return resolved?.Name ?? productType.Name;
    }

    private static ProductTypeDto MapProductTypeToDto(ProductType entity, string? defaultLanguage)
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

    [Authorize(CatalogPermissions.Catalog.Default)]
    public async Task DeleteAsync(Guid id)
    {
        var product = await _productRepository.GetAsync(id);
        var mediaList = await _mediaRepository.GetListAsync(m => m.ProductId == id);
        foreach (var m in mediaList)
            await _productMediaAppService.DeleteAsync(m.Id);
        var variants = await _variantRepository.GetListAsync(v => v.ProductId == id);
        foreach (var v in variants)
        {
            var inv = await _inventoryRepository.FirstOrDefaultAsync(i => i.ProductVariantId == v.Id);
            if (inv != null)
                await _inventoryRepository.DeleteAsync(inv);
            var attrs = await _variantAttributeRepository.GetListAsync(a => a.ProductVariantId == v.Id);
            foreach (var a in attrs)
                await _variantAttributeRepository.DeleteAsync(a);
            await _variantRepository.DeleteAsync(v);
        }
        await _productRepository.DeleteAsync(product);
    }

    /// <summary>
    /// Creates a variant with inventory and attributes. If attributeMap is provided, returns a DTO for the created variant (used by CreateAsync).
    /// </summary>
    private async Task<ProductVariantDto?> CreateVariantWithInventoryAsync(
        Guid productId,
        string sku,
        decimal? price,
        int quantity,
        Dictionary<string, object?> dynamicAttributes,
        List<ProductVariantAttributeInputDto> attributes,
        Dictionary<Guid, ProductAttribute>? attributeMap)
    {
        var variantId = GuidGenerator.Create();
        var variant = new ProductVariant(variantId, productId, sku, price, SerializeDynamicAttributes(dynamicAttributes));
        await _variantRepository.InsertAsync(variant);

        var invId = GuidGenerator.Create();
        var inv = new Inventory(invId, variantId, quantity, 0);
        await _inventoryRepository.InsertAsync(inv);

        foreach (var attr in attributes)
        {
            var attrId = GuidGenerator.Create();
            var va = new ProductVariantAttribute(attrId, variantId, attr.ProductAttributeId, attr.Value);
            await _variantAttributeRepository.InsertAsync(va);
        }

        if (attributeMap == null)
            return null;

        var attrDtos = attributes.Select(a => new ProductVariantAttributeDto
        {
            ProductAttributeId = a.ProductAttributeId,
            ProductAttributeName = attributeMap.TryGetValue(a.ProductAttributeId, out var att) ? att.Name : "",
            Value = a.Value
        }).ToList();

        return new ProductVariantDto
        {
            Id = variant.Id,
            ProductId = variant.ProductId,
            Sku = variant.Sku,
            Price = variant.Price,
            Quantity = inv.Quantity,
            Reserved = inv.Reserved,
            AvailableQuantity = inv.AvailableQuantity,
            DynamicAttributes = ParseDynamicAttributes(variant.DynamicAttributesJson),
            Attributes = attrDtos
        };
    }

    private async Task ValidateProductNumberAsync(string productNumber, Guid? excludeId)
    {
        var existing = await _productRepository.FirstOrDefaultAsync(p => p.ProductNumber == productNumber);
        if (existing != null && existing.Id != excludeId)
            throw new Volo.Abp.BusinessException("ECommerce:ProductNumberAlreadyExists").WithData("ProductNumber", productNumber);
    }

    private async Task ValidateModelBelongsToBrandAsync(Guid brandId, Guid? modelId)
    {
        if (!modelId.HasValue)
            return;
        var model = await _brandModelRepository.GetAsync(modelId.Value);
        if (model.BrandId != brandId)
            throw new Volo.Abp.BusinessException("ECommerce:ModelMustBelongToBrand");
    }

    private async Task ValidateDynamicAttributesAsync(Guid? productTypeId, Dictionary<string, object?> dynamicAttributes)
    {
        var normalizedAttributes = NormalizeDynamicAttributes(dynamicAttributes);
        if (!productTypeId.HasValue)
        {
            if (normalizedAttributes.Count > 0)
            {
                throw new Volo.Abp.BusinessException("ECommerce:DynamicAttributesRequireProductType");
            }

            return;
        }

        var productType = await CatalogTaxonomyAccess.GetVisibleEntityAsync(
            _productTypeRepository,
            productTypeId.Value,
            _multiTenantFilter,
            CurrentTenant,
            AsyncExecuter);
        if (!productType.IsActive)
        {
            throw new Volo.Abp.BusinessException("ECommerce:ProductTypeInactive")
                .WithData("ProductTypeId", productTypeId.Value);
        }

        var rules = await CatalogTaxonomyAccess.GetVisibleListAsync(
            _productTypeAttributeRuleRepository,
            _multiTenantFilter,
            CurrentTenant,
            AsyncExecuter,
            x => x.ProductTypeId == productTypeId.Value && x.TenantId == productType.TenantId);
        if (rules.Count == 0)
        {
            return;
        }

        var definitionIds = rules.Select(x => x.AttributeDefinitionId).Distinct().ToList();
        var definitions = await CatalogTaxonomyAccess.GetVisibleListAsync(
            _attributeDefinitionRepository,
            _multiTenantFilter,
            CurrentTenant,
            AsyncExecuter,
            x => definitionIds.Contains(x.Id));
        var definitionsById = definitions.ToDictionary(x => x.Id);

        foreach (var rule in rules)
        {
            if (!definitionsById.TryGetValue(rule.AttributeDefinitionId, out var definition))
            {
                continue;
            }

            if (!AttributeDefinitionCatalogGovernance.IsPublishedForCatalog(definition))
            {
                continue;
            }

            var key = definition.Key.Trim();
            normalizedAttributes.TryGetValue(key, out var rawValue);
            var stringValue = DynamicAttributeValueNormalizer.Normalize(rawValue);
            var ruleConditionSatisfied = ProductTypeAttributeRuleConditionEvaluator.IsRuleConditionSatisfied(rule, normalizedAttributes);

            if (definition.IsRequired && ruleConditionSatisfied && string.IsNullOrWhiteSpace(stringValue))
            {
                throw new Volo.Abp.BusinessException("ECommerce:DynamicAttributeRequired")
                    .WithData("AttributeKey", key);
            }

            if (string.IsNullOrWhiteSpace(stringValue))
            {
                continue;
            }

            ValidateAllowedValues(definition, key, stringValue);
            ValidateRegex(definition, key, stringValue);
            ValidateNumericRange(definition, key, stringValue);
        }
    }

    private static Dictionary<string, object?> NormalizeDynamicAttributes(Dictionary<string, object?>? dynamicAttributes)
    {
        if (dynamicAttributes == null || dynamicAttributes.Count == 0)
        {
            return new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        }

        return dynamicAttributes
            .Where(x => !string.IsNullOrWhiteSpace(x.Key))
            .ToDictionary(x => x.Key.Trim(), x => x.Value, StringComparer.OrdinalIgnoreCase);
    }

    private static string SerializeDynamicAttributes(Dictionary<string, object?>? dynamicAttributes)
    {
        var normalized = NormalizeDynamicAttributes(dynamicAttributes);
        return normalized.Count == 0 ? "{}" : JsonSerializer.Serialize(normalized);
    }

    private static void ValidateAllowedValues(AttributeDefinition definition, string key, string value)
    {
        if (string.IsNullOrWhiteSpace(definition.AllowedValuesJson))
        {
            return;
        }

        List<string>? allowedValues;
        try
        {
            allowedValues = JsonSerializer.Deserialize<List<string>>(definition.AllowedValuesJson);
        }
        catch (JsonException)
        {
            throw new Volo.Abp.BusinessException("ECommerce:DynamicAttributeAllowedValuesInvalidConfig")
                .WithData("AttributeKey", key);
        }

        var normalizedAllowed = (allowedValues ?? new List<string>())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (normalizedAllowed.Count == 0)
        {
            return;
        }

        if (!normalizedAllowed.Contains(value))
        {
            throw new Volo.Abp.BusinessException("ECommerce:DynamicAttributeInvalidAllowedValue")
                .WithData("AttributeKey", key)
                .WithData("AttributeValue", value);
        }
    }

    private static void ValidateRegex(AttributeDefinition definition, string key, string value)
    {
        if (string.IsNullOrWhiteSpace(definition.RegexPattern))
        {
            return;
        }

        try
        {
            if (!Regex.IsMatch(value, definition.RegexPattern))
            {
                throw new Volo.Abp.BusinessException("ECommerce:DynamicAttributeRegexMismatch")
                    .WithData("AttributeKey", key)
                    .WithData("AttributeValue", value);
            }
        }
        catch (ArgumentException)
        {
            throw new Volo.Abp.BusinessException("ECommerce:DynamicAttributeRegexInvalidConfig")
                .WithData("AttributeKey", key);
        }
    }

    private static void ValidateNumericRange(AttributeDefinition definition, string key, string value)
    {
        if (!definition.MinValue.HasValue && !definition.MaxValue.HasValue)
        {
            return;
        }

        if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var numericValue))
        {
            if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out numericValue))
            {
                throw new Volo.Abp.BusinessException("ECommerce:DynamicAttributeNumericExpected")
                    .WithData("AttributeKey", key)
                    .WithData("AttributeValue", value);
            }
        }

        if (definition.MinValue.HasValue && numericValue < definition.MinValue.Value)
        {
            throw new Volo.Abp.BusinessException("ECommerce:DynamicAttributeRangeViolation")
                .WithData("AttributeKey", key)
                .WithData("MinValue", definition.MinValue.Value)
                .WithData("AttributeValue", numericValue);
        }

        if (definition.MaxValue.HasValue && numericValue > definition.MaxValue.Value)
        {
            throw new Volo.Abp.BusinessException("ECommerce:DynamicAttributeRangeViolation")
                .WithData("AttributeKey", key)
                .WithData("MaxValue", definition.MaxValue.Value)
                .WithData("AttributeValue", numericValue);
        }
    }

    private async Task<string> GetDefaultLanguageAsync()
    {
        var configured = await _settingProvider.GetOrNullAsync(LocalizationSettingNames.DefaultLanguage);
        return string.IsNullOrWhiteSpace(configured) ? "en" : configured;
    }

    private async Task<List<Category>> LoadVisibleCategoriesWithDetailsAsync(List<Guid> categoryIds)
    {
        using (_multiTenantFilter.Disable())
        {
            var q = await _categoryRepository.WithDetailsAsync(x => x.Translations);
            return await AsyncExecuter.ToListAsync(
                q.Where(c => categoryIds.Contains(c.Id)).WhereVisibleToTaxonomyReader(CurrentTenant));
        }
    }

    private async Task<List<ProductType>> LoadVisibleProductTypesWithDetailsAsync(List<Guid> productTypeIds)
    {
        using (_multiTenantFilter.Disable())
        {
            var q = await _productTypeRepository.WithDetailsAsync(x => x.Translations);
            return await AsyncExecuter.ToListAsync(
                q.Where(x => productTypeIds.Contains(x.Id)).WhereVisibleToTaxonomyReader(CurrentTenant));
        }
    }
}
