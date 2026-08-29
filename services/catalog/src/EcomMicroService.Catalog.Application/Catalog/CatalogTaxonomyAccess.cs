using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using EcomMicroService.Catalog.Permissions;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Catalog;

/// <summary>
/// Host vs tenant taxonomy: shared rows use <see cref="IMultiTenant.TenantId"/> = null; tenant rows use the current tenant id.
/// ABP's default <see cref="IMultiTenant"/> filter hides cross-scope rows, so readers disable the filter and apply
/// <see cref="WhereVisibleToTaxonomyReader{T}"/>.
/// </summary>
public static class CatalogTaxonomyAccess
{
    public static IQueryable<T> WhereVisibleToTaxonomyReader<T>(this IQueryable<T> source, ICurrentTenant currentTenant)
        where T : class, IMultiTenant
    {
        if (!currentTenant.IsAvailable)
        {
            return source.Where(e => e.TenantId == null);
        }

        var tenantId = currentTenant.Id!.Value;
        return source.Where(e => e.TenantId == null || e.TenantId == tenantId);
    }

    /// <summary>
    /// Authorize creating a taxonomy row in the current scope. Use this before insert — <see cref="IMultiTenant.TenantId"/>
    /// may not be populated on the entity until the repository persists it.
    /// </summary>
    public static async Task EnsureCanCreateTaxonomyInCurrentScopeAsync(
        IPermissionChecker permissionChecker,
        ICurrentTenant currentTenant)
    {
        if (!currentTenant.IsAvailable)
        {
            if (!await permissionChecker.IsGrantedAsync(CatalogPermissions.Catalog.HostTaxonomy))
            {
                throw new AbpAuthorizationException(
                    "Creating shared (host) catalog taxonomy requires the host catalog taxonomy permission.");
            }

            return;
        }

        if (!await permissionChecker.IsGrantedAsync(CatalogPermissions.Catalog.Default))
        {
            throw new AbpAuthorizationException(
                "Creating tenant catalog taxonomy requires catalog permission.");
        }
    }

    public static async Task EnsureCanReadAdminTaxonomyAsync(IPermissionChecker permissionChecker)
    {
        if (await permissionChecker.IsGrantedAsync(CatalogPermissions.Catalog.Default))
        {
            return;
        }

        if (await permissionChecker.IsGrantedAsync(CatalogPermissions.Catalog.HostTaxonomy))
        {
            return;
        }

        throw new AbpAuthorizationException(
            "Catalog taxonomy read requires tenant catalog access or host catalog taxonomy permission.");
    }

    public static async Task EnsureCanMutateTaxonomyEntityAsync(
        IPermissionChecker permissionChecker,
        IMultiTenant entity,
        ICurrentTenant currentTenant)
    {
        if (entity.TenantId == null)
        {
            if (!await permissionChecker.IsGrantedAsync(CatalogPermissions.Catalog.HostTaxonomy))
            {
                throw new AbpAuthorizationException(
                    "Managing shared (host) catalog taxonomy requires the host catalog taxonomy permission.");
            }

            if (currentTenant.IsAvailable)
            {
                throw new AbpAuthorizationException(
                    "Host catalog taxonomy can only be modified in the host administration context (not while resolved to a tenant).");
            }

            return;
        }

        if (!await permissionChecker.IsGrantedAsync(CatalogPermissions.Catalog.Default))
        {
            throw new AbpAuthorizationException(
                "Managing tenant catalog taxonomy requires catalog permission.");
        }

        if (!currentTenant.IsAvailable || currentTenant.Id != entity.TenantId)
        {
            throw new AbpAuthorizationException(
                "You can only modify catalog taxonomy that belongs to the current tenant.");
        }
    }

    public static async Task<T> GetVisibleEntityAsync<T>(
        IRepository<T, Guid> repository,
        Guid id,
        IDataFilter<IMultiTenant> multiTenantFilter,
        ICurrentTenant currentTenant,
        IAsyncQueryableExecuter asyncExecuter,
        CancellationToken cancellationToken = default)
        where T : class, IEntity<Guid>, IMultiTenant
    {
        using (multiTenantFilter.Disable())
        {
            var queryable = await repository.GetQueryableAsync();
            var entity = await asyncExecuter.FirstOrDefaultAsync(
                queryable.Where(e => e.Id == id).WhereVisibleToTaxonomyReader(currentTenant),
                cancellationToken);
            if (entity == null)
            {
                throw new EntityNotFoundException(typeof(T), id);
            }

            return entity;
        }
    }

    public static async Task<List<T>> GetVisibleListAsync<T>(
        IRepository<T, Guid> repository,
        IDataFilter<IMultiTenant> multiTenantFilter,
        ICurrentTenant currentTenant,
        IAsyncQueryableExecuter asyncExecuter,
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
        where T : class, IEntity<Guid>, IMultiTenant
    {
        using (multiTenantFilter.Disable())
        {
            var queryable = await repository.GetQueryableAsync();
            if (predicate != null)
            {
                queryable = queryable.Where(predicate);
            }

            queryable = queryable.WhereVisibleToTaxonomyReader(currentTenant);
            return await asyncExecuter.ToListAsync(queryable, cancellationToken);
        }
    }
}
