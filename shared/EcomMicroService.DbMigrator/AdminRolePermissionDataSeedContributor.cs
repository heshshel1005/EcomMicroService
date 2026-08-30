using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;

namespace EcomMicroService.DbMigrator;

/// <summary>
/// Grants every defined permission to the <c>admin</c> role for the current host/tenant.
/// Administration does not otherwise see Catalog/Ordering permissions unless those
/// contracts are loaded, so tenant admins never received shop data permissions.
/// </summary>
public class AdminRolePermissionDataSeedContributor(
    IPermissionDefinitionManager permissionDefinitionManager,
    IPermissionDataSeeder permissionDataSeeder
) : IDataSeedContributor, ITransientDependency
{
    public async Task SeedAsync(DataSeedContext context)
    {
        var multiTenancySide = context.TenantId is null
            ? MultiTenancySides.Host
            : MultiTenancySides.Tenant;

        var permissions = await permissionDefinitionManager.GetPermissionsAsync();
        var permissionNames = permissions
            .Where(p => p.MultiTenancySide.HasFlag(multiTenancySide))
            .Where(p =>
                p.Providers.Count == 0
                || p.Providers.Contains(RolePermissionValueProvider.ProviderName)
            )
            .Select(p => p.Name)
            .ToArray();

        await permissionDataSeeder.SeedAsync(
            RolePermissionValueProvider.ProviderName,
            "admin",
            permissionNames,
            context.TenantId
        );
    }
}
