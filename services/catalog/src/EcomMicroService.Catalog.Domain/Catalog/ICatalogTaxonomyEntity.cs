using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Catalog;

/// <summary>
/// Marks catalog taxonomy roots that follow the host master-catalog convention:
/// <see cref="IMultiTenant.TenantId"/> <c>null</c> = shared host taxonomy visible to all tenants (read-only for tenants);
/// non-null = tenant-owned row visible only to that tenant (and host when not in a tenant context).
/// </summary>
public interface ICatalogTaxonomyEntity : IMultiTenant
{
}
