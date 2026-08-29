using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using EcomMicroService.Administration.EntityFrameworkCore;
using EcomMicroService.Basket.EntityFrameworkCore;
using EcomMicroService.Catalog.EntityFrameworkCore;
using EcomMicroService.IdentityService.EntityFrameworkCore;
using EcomMicroService.Ordering.EntityFrameworkCore;
using EcomMicroService.Projects.EntityFrameworkCore;
using EcomMicroService.Customer.EntityFrameworkCore;
using EcomMicroService.Payment.EntityFrameworkCore;
using EcomMicroService.Marketing.EntityFrameworkCore;
using EcomMicroService.Cms.EntityFrameworkCore;
using EcomMicroService.Notification.EntityFrameworkCore;
using EcomMicroService.SaaS.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Uow;

namespace EcomMicroService.DbMigrator;

public class EcomMicroServiceDbMigrationService(
    ILogger<EcomMicroServiceDbMigrationService> logger,
    ITenantRepository tenantRepository,
    IDataSeeder dataSeeder,
    ICurrentTenant currentTenant,
    IUnitOfWorkManager unitOfWorkManager
) : ITransientDependency
{
    private readonly ICurrentTenant _currentTenant = currentTenant;
    private readonly IDataSeeder _dataSeeder = dataSeeder;
    private readonly ILogger<EcomMicroServiceDbMigrationService> _logger = logger;
    private readonly ITenantRepository _tenantRepository = tenantRepository;
    private readonly IUnitOfWorkManager _unitOfWorkManager = unitOfWorkManager;

    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        await CreateDatabasesAsync(cancellationToken);

        _logger.LogInformation("Starting Migrations ...");
        await MigrateHostAsync(cancellationToken);
        await MigrateTenantsAsync(cancellationToken);
        _logger.LogInformation("Completed Migrations.");
    }

    private async Task CreateDatabasesAsync(CancellationToken cancellationToken)
    {
        await EnsureDatabaseAsync<SaaSDbContext>(cancellationToken);
        await EnsureDatabaseAsync<AdministrationDbContext>(cancellationToken);
        await EnsureDatabaseAsync<IdentityServiceDbContext>(cancellationToken);
        await EnsureDatabaseAsync<CatalogDbContext>(cancellationToken);
        await EnsureDatabaseAsync<BasketDbContext>(cancellationToken);
        await EnsureDatabaseAsync<OrderingDbContext>(cancellationToken);
        await EnsureDatabaseAsync<CustomerDbContext>(cancellationToken);
        await EnsureDatabaseAsync<PaymentDbContext>(cancellationToken);
        await EnsureDatabaseAsync<MarketingDbContext>(cancellationToken);
        await EnsureDatabaseAsync<CmsDbContext>(cancellationToken);
        await EnsureDatabaseAsync<NotificationDbContext>(cancellationToken);
    }

    private async Task MigrateHostAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Migrating Host side ...");
        await MigrateDatabasesAsync(null, cancellationToken);
        await SeedDataAsync(null);
        _logger.LogInformation("Host side migration completed.");
    }

    private async Task MigrateTenantsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Migrating tenants ...");

        var tenants = await _tenantRepository.GetListAsync(
            includeDetails: true,
            cancellationToken: cancellationToken
        );
        var migratedDatabaseSchemas = new HashSet<string>();

        foreach (var tenant in tenants)
        {
            using (_currentTenant.Change(tenant.Id))
            {
                // Database schema migration
                var connectionString = tenant.FindDefaultConnectionString();
                if (
                    !connectionString.IsNullOrWhiteSpace()
                    && //tenant has a separate database
                    !migratedDatabaseSchemas.Contains(connectionString)
                ) //the database was not migrated yet
                {
                    _logger.LogInformation(
                        "Migrating Tenant: {Name} ({TenantId})",
                        tenant.Name,
                        tenant.Id
                    );

                    await MigrateDatabasesAsync(tenant, cancellationToken);
                    migratedDatabaseSchemas.AddIfNotContains(connectionString);
                }

                //Seed data
                await SeedDataAsync(tenant);
            }
        }

        _logger.LogInformation("Tenant migrations are complete.");
    }

    private async Task EnsureDatabaseAsync<TDbContext>(CancellationToken cancellationToken)
        where TDbContext : DbContext, IEfCoreDbContext
    {
        using var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false);

        var dbContext = await _unitOfWorkManager
            .Current!.ServiceProvider.GetRequiredService<IDbContextProvider<TDbContext>>()
            .GetDbContextAsync();

        var strategy = dbContext.Database.CreateExecutionStrategy();

        var dbCreator = dbContext.GetService<IRelationalDatabaseCreator>();

        await strategy.ExecuteAsync(async () =>
        {
            // Create the database if it does not exist.
            // Do this first so there is then a database to start a transaction against.
            if (!await dbCreator.ExistsAsync(cancellationToken))
            {
                await dbCreator.CreateAsync(cancellationToken);
            }
        });

        await uow.CompleteAsync(cancellationToken);
    }

    private async Task MigrateDatabasesAsync(Tenant? tenant, CancellationToken cancellationToken)
    {
        if (tenant is null)
        {
            /* SaaS schema should only be available in the host side */
            await MigrateDatabaseAsync<SaaSDbContext>(cancellationToken);
        }

        await MigrateDatabaseAsync<AdministrationDbContext>(cancellationToken);
        await MigrateDatabaseAsync<IdentityServiceDbContext>(cancellationToken);
        await MigrateDatabaseAsync<CatalogDbContext>(cancellationToken);
        await MigrateDatabaseAsync<BasketDbContext>(cancellationToken);
        await MigrateDatabaseAsync<OrderingDbContext>(cancellationToken);
        await MigrateDatabaseAsync<CustomerDbContext>(cancellationToken);
        await MigrateDatabaseAsync<PaymentDbContext>(cancellationToken);
        await MigrateDatabaseAsync<MarketingDbContext>(cancellationToken);
        await MigrateDatabaseAsync<CmsDbContext>(cancellationToken);
        await MigrateDatabaseAsync<NotificationDbContext>(cancellationToken);
        await MigrateDatabaseAsync<ProjectsDbContext>(cancellationToken);
        //await MigrateDatabaseAsync<WebAppDbContext>(cancellationToken);
    }

    private async Task MigrateDatabaseAsync<TDbContext>(CancellationToken cancellationToken)
        where TDbContext : DbContext, IEfCoreDbContext
    {
        var name = typeof(TDbContext).Name.RemovePostFix("DbContext");

        _logger.LogInformation("Migrating {Name} database ...", name);

        using var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false);

        var dbContext = await _unitOfWorkManager
            .Current!.ServiceProvider.GetRequiredService<IDbContextProvider<TDbContext>>()
            .GetDbContextAsync();

        await ApplyMigrationAsync(dbContext, cancellationToken);

        await uow.CompleteAsync(cancellationToken);

        _logger.LogInformation("Completed migrating ({Name}).", name);
    }

    private static async Task ApplyMigrationAsync<TDbContext>(
        TDbContext dbContext,
        CancellationToken cancellationToken
    )
        where TDbContext : DbContext, IEfCoreDbContext
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        });
    }

    private async Task SeedDataAsync(Tenant? tenant)
    {
        if (tenant is null)
        {
            _logger.LogInformation("Seeding host data ...");
        }
        else
        {
            _logger.LogInformation("Seeding tenant data: {Name} ({Id})", tenant.Name, tenant.Id);
        }

        await _dataSeeder.SeedAsync(
            new DataSeedContext(tenant?.Id)
                .WithProperty(
                    IdentityDataSeedContributor.AdminEmailPropertyName,
                    IdentityDataSeedContributor.AdminEmailDefaultValue
                )
                .WithProperty(
                    IdentityDataSeedContributor.AdminPasswordPropertyName,
                    IdentityDataSeedContributor.AdminPasswordDefaultValue
                )
        );
    }
}
