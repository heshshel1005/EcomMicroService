using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Catalog;

public class ProductTypeAttributeRuleAppService : CatalogAppService, IProductTypeAttributeRuleAppService
{
    private readonly IRepository<ProductType, Guid> _productTypeRepository;
    private readonly IRepository<AttributeDefinition, Guid> _attributeDefinitionRepository;
    private readonly IRepository<ProductTypeAttributeRule, Guid> _ruleRepository;
    private readonly IDataFilter<IMultiTenant> _multiTenantFilter;

    public ProductTypeAttributeRuleAppService(
        IRepository<ProductType, Guid> productTypeRepository,
        IRepository<AttributeDefinition, Guid> attributeDefinitionRepository,
        IRepository<ProductTypeAttributeRule, Guid> ruleRepository,
        IDataFilter<IMultiTenant> multiTenantFilter)
    {
        _productTypeRepository = productTypeRepository;
        _attributeDefinitionRepository = attributeDefinitionRepository;
        _ruleRepository = ruleRepository;
        _multiTenantFilter = multiTenantFilter;
    }

    [Authorize]
    public async Task<List<ProductTypeAttributeRuleDto>> GetListByProductTypeAsync(Guid productTypeId)
    {
        await CatalogTaxonomyAccess.EnsureCanReadAdminTaxonomyAsync(PermissionChecker);
        var productType = await CatalogTaxonomyAccess.GetVisibleEntityAsync(
            _productTypeRepository,
            productTypeId,
            _multiTenantFilter,
            CurrentTenant,
            AsyncExecuter);
        var list = await CatalogTaxonomyAccess.GetVisibleListAsync(
            _ruleRepository,
            _multiTenantFilter,
            CurrentTenant,
            AsyncExecuter,
            x => x.ProductTypeId == productTypeId && x.TenantId == productType.TenantId);
        return list
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.CreationTime)
            .Select(x => new ProductTypeAttributeRuleDto
            {
                Id = x.Id,
                ProductTypeId = x.ProductTypeId,
                AttributeDefinitionId = x.AttributeDefinitionId,
                DisplayOrder = x.DisplayOrder,
                ConditionalAttributeKey = x.ConditionalAttributeKey,
                ConditionalOperator = x.ConditionalOperator,
                ConditionalExpectedValue = x.ConditionalExpectedValue
            })
            .ToList();
    }

    [Authorize]
    public async Task ReplaceForProductTypeAsync(Guid productTypeId, List<UpdateProductTypeAttributeRuleDto> input)
    {
        await CatalogTaxonomyAccess.EnsureCanReadAdminTaxonomyAsync(PermissionChecker);
        var productType = await CatalogTaxonomyAccess.GetVisibleEntityAsync(
            _productTypeRepository,
            productTypeId,
            _multiTenantFilter,
            CurrentTenant,
            AsyncExecuter);
        await CatalogTaxonomyAccess.EnsureCanMutateTaxonomyEntityAsync(PermissionChecker, productType, CurrentTenant);

        var normalized = (input ?? new List<UpdateProductTypeAttributeRuleDto>())
            .Where(x => x.AttributeDefinitionId != Guid.Empty)
            .GroupBy(x => x.AttributeDefinitionId)
            .Select(g => g.OrderBy(x => x.DisplayOrder).First())
            .ToList();

        var definitionIds = normalized.Select(x => x.AttributeDefinitionId).Distinct().ToList();
        if (definitionIds.Count > 0)
        {
            var existingDefinitions = await CatalogTaxonomyAccess.GetVisibleListAsync(
                _attributeDefinitionRepository,
                _multiTenantFilter,
                CurrentTenant,
                AsyncExecuter,
                x => definitionIds.Contains(x.Id));
            if (existingDefinitions.Count != definitionIds.Count)
            {
                throw new Volo.Abp.BusinessException("ECommerce:InvalidAttributeDefinition");
            }

            var unpublished = existingDefinitions
                .Where(d => !AttributeDefinitionCatalogGovernance.IsPublishedForCatalog(d))
                .Select(d => d.Key)
                .ToList();
            if (unpublished.Count > 0)
            {
                throw new BusinessException(CatalogDomainErrorCodes.AttributeDefinitionMustBePublishedForProductTypeRules)
                    .WithData("Keys", string.Join(", ", unpublished));
            }
        }

        var existing = await CatalogTaxonomyAccess.GetVisibleListAsync(
            _ruleRepository,
            _multiTenantFilter,
            CurrentTenant,
            AsyncExecuter,
            x => x.ProductTypeId == productTypeId && x.TenantId == productType.TenantId);
        foreach (var rule in existing)
        {
            await _ruleRepository.DeleteAsync(rule);
        }

        foreach (var item in normalized.OrderBy(x => x.DisplayOrder))
        {
            var rule = new ProductTypeAttributeRule(
                GuidGenerator.Create(),
                productTypeId,
                item.AttributeDefinitionId,
                item.DisplayOrder);
            rule.TenantId = productType.TenantId;
            await _ruleRepository.InsertAsync(rule);
        }
    }
}
