using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EcomMicroService.Catalog.MultiTenancy;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Uow;

namespace EcomMicroService.Catalog;

/// <summary>
/// Seeds default product attributes (Size, Color) for catalog admin.
/// When multi-tenancy is enabled, runs only for a specific tenant (not the host) so each store tenant gets its own reference rows.
/// </summary>
public class CatalogDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<ProductAttribute, System.Guid> _attributeRepository;
    private readonly IRepository<ProductType, Guid> _productTypeRepository;
    private readonly IRepository<AttributeDefinition, Guid> _attributeDefinitionRepository;
    private readonly IRepository<AttributeOption, Guid> _attributeOptionRepository;
    private readonly IRepository<ProductTypeAttributeRule, Guid> _productTypeAttributeRuleRepository;
    private readonly IRepository<Category, Guid> _categoryRepository;
    private readonly IGuidGenerator _guidGenerator;

    public CatalogDataSeedContributor(
        IRepository<ProductAttribute, System.Guid> attributeRepository,
        IRepository<ProductType, Guid> productTypeRepository,
        IRepository<AttributeDefinition, Guid> attributeDefinitionRepository,
        IRepository<AttributeOption, Guid> attributeOptionRepository,
        IRepository<ProductTypeAttributeRule, Guid> productTypeAttributeRuleRepository,
        IRepository<Category, Guid> categoryRepository,
        IGuidGenerator guidGenerator)
    {
        _attributeRepository = attributeRepository;
        _productTypeRepository = productTypeRepository;
        _attributeDefinitionRepository = attributeDefinitionRepository;
        _attributeOptionRepository = attributeOptionRepository;
        _productTypeAttributeRuleRepository = productTypeAttributeRuleRepository;
        _categoryRepository = categoryRepository;
        _guidGenerator = guidGenerator;
    }

    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        if (MultiTenancyConsts.IsEnabled && !context.TenantId.HasValue)
        {
            return;
        }

        await SeedVariantAttributesAsync();
        await SeedAutoPartProductTypeAsync(context.TenantId);
        await SeedDefaultCategoriesAsync(context.TenantId);
    }

    private async Task SeedVariantAttributesAsync()
    {
        var existingNames = (await _attributeRepository.GetListAsync())
            .Select(x => x.Name.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!existingNames.Contains("Size"))
        {
            await _attributeRepository.InsertAsync(new ProductAttribute(_guidGenerator.Create(), "Size"));
        }

        if (!existingNames.Contains("Color"))
        {
            await _attributeRepository.InsertAsync(new ProductAttribute(_guidGenerator.Create(), "Color"));
        }
    }

    private async Task SeedAutoPartProductTypeAsync(Guid? tenantId)
    {
        var productType = await _productTypeRepository.FirstOrDefaultAsync(x => x.Code == "AUTO_PART");
        if (productType == null)
        {
            var productTypeId = _guidGenerator.Create();
            productType = new ProductType(productTypeId, "AUTO_PART", "Auto Part");
            productType.TenantId = tenantId;
            const string seedDefaultLanguage = "en";
            productType.SetTranslations(
                new[]
                {
                    new ProductTypeTranslation(_guidGenerator.Create(), productTypeId, seedDefaultLanguage, "Auto Part")
                },
                seedDefaultLanguage);
            await _productTypeRepository.InsertAsync(productType);
        }

        var partNumber = await EnsureAttributeDefinitionAsync(
            tenantId, "part_number", AttributeDefinitionDataType.Text, null, null, isRequired: true);
        var condition = await EnsureAttributeDefinitionAsync(
            tenantId, "condition", AttributeDefinitionDataType.Enum, new[] { "new", "used", "remanufactured" }, null, isRequired: true);
        var gtinUpc = await EnsureAttributeDefinitionAsync(
            tenantId, "gtin_upc", AttributeDefinitionDataType.Text, null, "^\\d{12,14}$", isRecommended: true);
        var fitmentType = await EnsureAttributeDefinitionAsync(
            tenantId, "fitment_type", AttributeDefinitionDataType.Enum, new[] { "universal", "vehicle_specific", "custom" }, null, isRecommended: true);

        await EnsureAttributeOptionsAsync(condition);
        await EnsureAttributeOptionsAsync(fitmentType);

        await EnsureRuleAsync(tenantId, productType.Id, partNumber.Id, 10);
        await EnsureRuleAsync(tenantId, productType.Id, condition.Id, 20);
        await EnsureRuleAsync(tenantId, productType.Id, gtinUpc.Id, 30);
        await EnsureRuleAsync(tenantId, productType.Id, fitmentType.Id, 40);
    }

    private async Task<AttributeDefinition> EnsureAttributeDefinitionAsync(
        Guid? tenantId,
        string key,
        AttributeDefinitionDataType dataType,
        IEnumerable<string>? allowedValues,
        string? regexPattern,
        bool isRequired = false,
        bool isRecommended = false)
    {
        var definition = await _attributeDefinitionRepository.FirstOrDefaultAsync(x => x.Key == key);
        if (definition != null)
        {
            if (definition.GovernanceStatus != AttributeDefinitionGovernanceStatus.Published)
            {
                definition.Publish();
                await _attributeDefinitionRepository.UpdateAsync(definition);
            }

            return definition;
        }

        var allowedValuesJson = allowedValues == null
            ? null
            : JsonSerializer.Serialize(allowedValues);

        definition = new AttributeDefinition(
            _guidGenerator.Create(),
            key,
            dataType,
            allowedValuesJson,
            regexPattern,
            minValue: null,
            maxValue: null,
            isRequired,
            isRecommended);
        definition.TenantId = tenantId;
        definition.Publish();

        await _attributeDefinitionRepository.InsertAsync(definition);
        return definition;
    }

    private async Task EnsureAttributeOptionsAsync(AttributeDefinition definition)
    {
        if (definition.DataType != AttributeDefinitionDataType.Enum)
        {
            return;
        }

        var ordered = AttributeAllowedValuesParser.ParseOrdered(definition.AllowedValuesJson);
        if (ordered.Count == 0)
        {
            return;
        }

        var existing = await _attributeOptionRepository.GetListAsync(x => x.AttributeDefinitionId == definition.Id);
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
                var opt = new AttributeOption(_guidGenerator.Create(), definition.Id, v, i);
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

    private async Task EnsureRuleAsync(Guid? tenantId, Guid productTypeId, Guid attributeDefinitionId, int displayOrder)
    {
        var existingRule = await _productTypeAttributeRuleRepository.FirstOrDefaultAsync(x =>
            x.ProductTypeId == productTypeId && x.AttributeDefinitionId == attributeDefinitionId);
        if (existingRule != null)
        {
            return;
        }

        var rule = new ProductTypeAttributeRule(
            _guidGenerator.Create(),
            productTypeId,
            attributeDefinitionId,
            displayOrder);
        rule.TenantId = tenantId;
        await _productTypeAttributeRuleRepository.InsertAsync(rule);
    }

    private async Task SeedDefaultCategoriesAsync(Guid? tenantId)
    {
        if (await _categoryRepository.GetCountAsync() > 0)
        {
            return;
        }

        var seeds = new (string Name, string Slug, int DisplayOrder)[]
        {
            ("General", "general", 0),
            ("Electronics", "electronics", 10),
            ("Accessories", "accessories", 20),
        };

        const string defaultLanguage = "en";
        foreach (var seed in seeds)
        {
            var id = _guidGenerator.Create();
            var category = new Category(id, seed.Name, seed.Slug, parentId: null, seed.DisplayOrder)
            {
                TenantId = tenantId
            };
            category.SetTranslations(
                new[]
                {
                    new CategoryTranslation(_guidGenerator.Create(), id, defaultLanguage, seed.Name)
                },
                defaultLanguage);
            await _categoryRepository.InsertAsync(category);
        }
    }
}
