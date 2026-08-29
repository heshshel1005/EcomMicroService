using Microsoft.Extensions.Hosting;
using Projects;

namespace EcomMicroService.AppHost;

internal class Program
{
    private static void Main(string[] args)
    {
        const string LaunchProfileName = "Aspire";
        var builder = DistributedApplication.CreateBuilder(args);

        var postgres = builder.AddPostgres(EcomMicroServiceNames.Postgres)
            .WithDataVolume()
            .WithPgWeb()
            .WithPgAdmin(pgAdmin => pgAdmin.WithImageTag("9.17"));
        var rabbitMq = builder.AddRabbitMQ(EcomMicroServiceNames.RabbitMq).WithManagementPlugin();
        var redis = builder.AddRedis(EcomMicroServiceNames.Redis).WithRedisCommander();
        var seq = builder.AddSeq(EcomMicroServiceNames.Seq);

        var adminDb = postgres.AddDatabase(EcomMicroServiceNames.AdministrationDb);
        var identityDb = postgres.AddDatabase(EcomMicroServiceNames.IdentityServiceDb);
        var projectsDb = postgres.AddDatabase(EcomMicroServiceNames.ProjectsDb);
        var saasDb = postgres.AddDatabase(EcomMicroServiceNames.SaaSDb);
        var catalogDb = postgres.AddDatabase(EcomMicroServiceNames.CatalogDb);
        var basketDb = postgres.AddDatabase(EcomMicroServiceNames.BasketDb);
        var orderingDb = postgres.AddDatabase(EcomMicroServiceNames.OrderingDb);
        var customerDb = postgres.AddDatabase(EcomMicroServiceNames.CustomerDb);
        var paymentDb = postgres.AddDatabase(EcomMicroServiceNames.PaymentDb);
        var marketingDb = postgres.AddDatabase(EcomMicroServiceNames.MarketingDb);
        var cmsDb = postgres.AddDatabase(EcomMicroServiceNames.CmsDb);
        var notificationDb = postgres.AddDatabase(EcomMicroServiceNames.NotificationDb);

        var migrator = builder
            .AddProject<EcomMicroService_DbMigrator>(
                EcomMicroServiceNames.DbMigrator,
                launchProfileName: LaunchProfileName
            )
            .WithReference(adminDb)
            .WithReference(identityDb)
            .WithReference(projectsDb)
            .WithReference(saasDb)
            .WithReference(catalogDb)
            .WithReference(basketDb)
            .WithReference(orderingDb)
            .WithReference(customerDb)
            .WithReference(paymentDb)
            .WithReference(marketingDb)
            .WithReference(cmsDb)
            .WithReference(notificationDb)
            .WithReference(seq)
            .WaitFor(postgres);

        var admin = builder
            .AddProject<EcomMicroService_Administration_HttpApi_Host>(
                EcomMicroServiceNames.AdministrationApi,
                launchProfileName: LaunchProfileName
            )
            .WithExternalHttpEndpoints()
            .WithReference(adminDb)
            .WithReference(identityDb)
            .WithReference(saasDb)
            .WithReference(rabbitMq)
            .WithReference(redis)
            .WithReference(seq)
            .WaitForCompletion(migrator);

        var identity = builder
            .AddProject<EcomMicroService_IdentityService_HttpApi_Host>(
                EcomMicroServiceNames.IdentityServiceApi,
                launchProfileName: LaunchProfileName
            )
            .WithExternalHttpEndpoints()
            .WithReference(adminDb)
            .WithReference(identityDb)
            .WithReference(saasDb)
            .WithReference(rabbitMq)
            .WithReference(redis)
            .WithReference(seq)
            .WaitForCompletion(migrator);

        var saas = builder
            .AddProject<EcomMicroService_SaaS_HttpApi_Host>(
                EcomMicroServiceNames.SaaSApi,
                launchProfileName: LaunchProfileName
            )
            .WithExternalHttpEndpoints()
            .WithReference(adminDb)
            .WithReference(saasDb)
            .WithReference(rabbitMq)
            .WithReference(redis)
            .WithReference(seq)
            .WaitForCompletion(migrator);

        builder
            .AddProject<EcomMicroService_Projects_HttpApi_Host>(
                EcomMicroServiceNames.ProjectsApi,
                launchProfileName: LaunchProfileName
            )
            .WithExternalHttpEndpoints()
            .WithReference(adminDb)
            .WithReference(projectsDb)
            .WithReference(rabbitMq)
            .WithReference(redis)
            .WithReference(seq)
            .WaitForCompletion(migrator);

        var catalog = builder
            .AddProject<EcomMicroService_Catalog_HttpApi_Host>(
                EcomMicroServiceNames.CatalogApi,
                launchProfileName: LaunchProfileName
            )
            .WithExternalHttpEndpoints()
            .WithReference(adminDb)
            .WithReference(catalogDb)
            .WithReference(rabbitMq)
            .WithReference(redis)
            .WithReference(seq)
            .WaitForCompletion(migrator);

        var basket = builder
            .AddProject<EcomMicroService_Basket_HttpApi_Host>(
                EcomMicroServiceNames.BasketApi,
                launchProfileName: LaunchProfileName
            )
            .WithExternalHttpEndpoints()
            .WithReference(adminDb)
            .WithReference(basketDb)
            .WithReference(rabbitMq)
            .WithReference(redis)
            .WithReference(seq)
            .WaitForCompletion(migrator);

        var ordering = builder
            .AddProject<EcomMicroService_Ordering_HttpApi_Host>(
                EcomMicroServiceNames.OrderingApi,
                launchProfileName: LaunchProfileName
            )
            .WithExternalHttpEndpoints()
            .WithReference(adminDb)
            .WithReference(orderingDb)
            .WithReference(rabbitMq)
            .WithReference(redis)
            .WithReference(seq)
            .WaitForCompletion(migrator);

        var customer = builder
            .AddProject<EcomMicroService_Customer_HttpApi_Host>(
                EcomMicroServiceNames.CustomerApi,
                launchProfileName: LaunchProfileName
            )
            .WithExternalHttpEndpoints()
            .WithReference(adminDb)
            .WithReference(customerDb)
            .WithReference(rabbitMq)
            .WithReference(redis)
            .WithReference(seq)
            .WaitForCompletion(migrator);

        var payment = builder
            .AddProject<EcomMicroService_Payment_HttpApi_Host>(
                EcomMicroServiceNames.PaymentApi,
                launchProfileName: LaunchProfileName
            )
            .WithExternalHttpEndpoints()
            .WithReference(adminDb)
            .WithReference(paymentDb)
            .WithReference(rabbitMq)
            .WithReference(redis)
            .WithReference(seq)
            .WaitForCompletion(migrator);

        var marketing = builder
            .AddProject<EcomMicroService_Marketing_HttpApi_Host>(
                EcomMicroServiceNames.MarketingApi,
                launchProfileName: LaunchProfileName
            )
            .WithExternalHttpEndpoints()
            .WithReference(adminDb)
            .WithReference(marketingDb)
            .WithReference(rabbitMq)
            .WithReference(redis)
            .WithReference(seq)
            .WaitForCompletion(migrator);

        var cms = builder
            .AddProject<EcomMicroService_Cms_HttpApi_Host>(
                EcomMicroServiceNames.CmsApi,
                launchProfileName: LaunchProfileName
            )
            .WithExternalHttpEndpoints()
            .WithReference(adminDb)
            .WithReference(cmsDb)
            .WithReference(rabbitMq)
            .WithReference(redis)
            .WithReference(seq)
            .WaitForCompletion(migrator);

        var notification = builder
            .AddProject<EcomMicroService_Notification_HttpApi_Host>(
                EcomMicroServiceNames.NotificationApi,
                launchProfileName: LaunchProfileName
            )
            .WithExternalHttpEndpoints()
            .WithReference(adminDb)
            .WithReference(notificationDb)
            .WithReference(rabbitMq)
            .WithReference(redis)
            .WithReference(seq)
            .WaitForCompletion(migrator);

        var gateway = builder
            .AddProject<EcomMicroService_Gateway>(EcomMicroServiceNames.Gateway, launchProfileName: LaunchProfileName)
            .WithExternalHttpEndpoints()
            .WithReference(seq)
            .WaitFor(admin)
            .WaitFor(identity)
            .WaitFor(saas)
            .WaitFor(catalog)
            .WaitFor(basket)
            .WaitFor(ordering)
            .WaitFor(customer)
            .WaitFor(payment)
            .WaitFor(marketing)
            .WaitFor(cms)
            .WaitFor(notification);

        var authserver = builder
            .AddProject<EcomMicroService_AuthServer>(
                EcomMicroServiceNames.AuthServer,
                launchProfileName: LaunchProfileName
            )
            .WithExternalHttpEndpoints()
            .WithReference(adminDb)
            .WithReference(identityDb)
            .WithReference(saasDb)
            .WithReference(rabbitMq)
            .WithReference(redis)
            .WithReference(seq)
            .WaitForCompletion(migrator);

        builder
            .AddProject<EcomMicroService_WebApp_Blazor>(
                EcomMicroServiceNames.WebAppClient,
                launchProfileName: LaunchProfileName
            )
            .WithExternalHttpEndpoints()
            .WithReference(seq)
            .WaitFor(authserver)
            .WaitFor(gateway);

        builder.Build().Run();
    }
}
