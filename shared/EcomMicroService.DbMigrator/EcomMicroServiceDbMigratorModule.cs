using EcomMicroService.Administration;
using EcomMicroService.Administration.EntityFrameworkCore;
using EcomMicroService.Catalog;
using EcomMicroService.Catalog.EntityFrameworkCore;
using EcomMicroService.Basket;
using EcomMicroService.Basket.EntityFrameworkCore;
using EcomMicroService.Ordering;
using EcomMicroService.Ordering.EntityFrameworkCore;
using EcomMicroService.IdentityService;
using EcomMicroService.IdentityService.EntityFrameworkCore;
using EcomMicroService.Projects;
using EcomMicroService.Projects.EntityFrameworkCore;
using EcomMicroService.Customer;
using EcomMicroService.Customer.EntityFrameworkCore;
using EcomMicroService.Payment;
using EcomMicroService.Payment.EntityFrameworkCore;
using EcomMicroService.Marketing;
using EcomMicroService.Marketing.EntityFrameworkCore;
using EcomMicroService.Cms;
using EcomMicroService.Cms.EntityFrameworkCore;
using EcomMicroService.Notification;
using EcomMicroService.Notification.EntityFrameworkCore;
using EcomMicroService.SaaS;
using EcomMicroService.SaaS.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict.Tokens;

namespace EcomMicroService.DbMigrator;

[DependsOn(typeof(AbpAutofacModule))]
[DependsOn(typeof(AbpBackgroundJobsAbstractionsModule))]
[DependsOn(typeof(AdministrationEntityFrameworkCoreModule))]
[DependsOn(typeof(AdministrationApplicationContractsModule))]
[DependsOn(typeof(IdentityServiceEntityFrameworkCoreModule))]
[DependsOn(typeof(IdentityServiceApplicationContractsModule))]
[DependsOn(typeof(CatalogEntityFrameworkCoreModule))]
[DependsOn(typeof(CatalogApplicationContractsModule))]
[DependsOn(typeof(BasketEntityFrameworkCoreModule))]
[DependsOn(typeof(BasketApplicationContractsModule))]
[DependsOn(typeof(OrderingEntityFrameworkCoreModule))]
[DependsOn(typeof(OrderingApplicationContractsModule))]
[DependsOn(typeof(CustomerEntityFrameworkCoreModule))]
[DependsOn(typeof(CustomerApplicationContractsModule))]
[DependsOn(typeof(PaymentEntityFrameworkCoreModule))]
[DependsOn(typeof(PaymentApplicationContractsModule))]
[DependsOn(typeof(MarketingEntityFrameworkCoreModule))]
[DependsOn(typeof(MarketingApplicationContractsModule))]
[DependsOn(typeof(CmsEntityFrameworkCoreModule))]
[DependsOn(typeof(CmsApplicationContractsModule))]
[DependsOn(typeof(NotificationEntityFrameworkCoreModule))]
[DependsOn(typeof(NotificationApplicationContractsModule))]
[DependsOn(typeof(ProjectsEntityFrameworkCoreModule))]
[DependsOn(typeof(ProjectsApplicationContractsModule))]
[DependsOn(typeof(SaaSEntityFrameworkCoreModule))]
[DependsOn(typeof(SaaSApplicationContractsModule))]
//[DependsOn(typeof(WebAppEntityFrameworkCoreModule))]
//[DependsOn(typeof(WebAppApplicationContractsModule))]
public class EcomMicroServiceDbMigratorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpBackgroundJobOptions>(options => options.IsJobExecutionEnabled = false);
        Configure<TokenCleanupOptions>(options => options.IsCleanupEnabled = false);
    }
}
