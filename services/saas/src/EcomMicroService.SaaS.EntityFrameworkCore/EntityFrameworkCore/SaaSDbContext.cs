using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace EcomMicroService.SaaS.EntityFrameworkCore;

[ConnectionStringName(EcomMicroServiceNames.SaaSDb)]
public class SaaSDbContext(DbContextOptions<SaaSDbContext> options)
    : AbpDbContext<SaaSDbContext>(options),
        ITenantManagementDbContext,
        ISaaSDbContext
{
    public DbSet<Tenant> Tenants { get; set; }

    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    public DbSet<OrganizationSignupRequest> OrganizationSignupRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureSaaS();
        builder.ConfigureTenantManagement();
        builder.Entity<OrganizationSignupRequest>(b =>
        {
            b.ToTable(SaaSDbProperties.DbTablePrefix + "OrganizationSignupRequests", SaaSDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.TenantName).IsRequired().HasMaxLength(64);
            b.Property(x => x.AdminEmail).HasMaxLength(256);
            b.HasIndex(x => x.Status);
        });
    }
}
