using Serilog;
using EcomMicroService.Administration.EntityFrameworkCore;
using EcomMicroService.Projects.EntityFrameworkCore;
using EcomMicroService.SaaS.EntityFrameworkCore;
using EcomMicroService.IdentityService.EntityFrameworkCore;
using EcomMicroService.Catalog.EntityFrameworkCore;
using EcomMicroService.Basket.EntityFrameworkCore;
using EcomMicroService.Ordering.EntityFrameworkCore;
using EcomMicroService.Customer.EntityFrameworkCore;
using EcomMicroService.Payment.EntityFrameworkCore;
using EcomMicroService.Marketing.EntityFrameworkCore;
using EcomMicroService.Cms.EntityFrameworkCore;
using EcomMicroService.Notification.EntityFrameworkCore;

namespace EcomMicroService.DbMigrator;

internal class Program
{
    private static async Task Main(string[] args)
    {
        EcomMicroServiceLogging.Initialize();

        var builder = Host.CreateApplicationBuilder(args);

        builder.AddServiceDefaults();

        builder.AddNpgsqlDbContext<AdministrationDbContext>(
            connectionName: EcomMicroServiceNames.AdministrationDb,
            configure => configure.DisableRetry = true
        );
        builder.AddNpgsqlDbContext<IdentityServiceDbContext>(
            connectionName: EcomMicroServiceNames.IdentityServiceDb,
            configure => configure.DisableRetry = true
        );
        builder.AddNpgsqlDbContext<SaaSDbContext>(
            connectionName: EcomMicroServiceNames.SaaSDb,
            configure => configure.DisableRetry = true
        );
        builder.AddNpgsqlDbContext<ProjectsDbContext>(
            connectionName: EcomMicroServiceNames.ProjectsDb,
            configure => configure.DisableRetry = true
        );
        builder.AddNpgsqlDbContext<CatalogDbContext>(
            connectionName: EcomMicroServiceNames.CatalogDb,
            configure => configure.DisableRetry = true
        );
        builder.AddNpgsqlDbContext<BasketDbContext>(
            connectionName: EcomMicroServiceNames.BasketDb,
            configure => configure.DisableRetry = true
        );
        builder.AddNpgsqlDbContext<OrderingDbContext>(
            connectionName: EcomMicroServiceNames.OrderingDb,
            configure => configure.DisableRetry = true
        );
        builder.AddNpgsqlDbContext<CustomerDbContext>(
            connectionName: EcomMicroServiceNames.CustomerDb,
            configure => configure.DisableRetry = true
        );
        builder.AddNpgsqlDbContext<PaymentDbContext>(
            connectionName: EcomMicroServiceNames.PaymentDb,
            configure => configure.DisableRetry = true
        );
        builder.AddNpgsqlDbContext<MarketingDbContext>(
            connectionName: EcomMicroServiceNames.MarketingDb,
            configure => configure.DisableRetry = true
        );
        builder.AddNpgsqlDbContext<CmsDbContext>(
            connectionName: EcomMicroServiceNames.CmsDb,
            configure => configure.DisableRetry = true
        );
        builder.AddNpgsqlDbContext<NotificationDbContext>(
            connectionName: EcomMicroServiceNames.NotificationDb,
            configure => configure.DisableRetry = true
        );

        builder.Configuration.AddAppSettingsSecretsJson();

        builder.Logging.AddSerilog();

        builder.Services.AddHostedService<DbMigratorHostedService>();

        var host = builder.Build();

        await host.RunAsync();
    }
}
