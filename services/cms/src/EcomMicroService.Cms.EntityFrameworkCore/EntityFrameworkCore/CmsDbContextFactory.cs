using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace EcomMicroService.Cms.EntityFrameworkCore;

public class CmsDbContextFactory : IDesignTimeDbContextFactory<CmsDbContext>
{
    public CmsDbContext CreateDbContext(string[] args)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        CmsGlobalFeatureConfigurator.Configure();
        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<CmsDbContext>().UseNpgsql(
            GetConnectionStringFromConfiguration()
        );

        return new CmsDbContext(builder.Options);
    }

    private static string GetConnectionStringFromConfiguration()
    {
        return BuildConfiguration().GetConnectionString(CmsDbProperties.ConnectionStringName) ?? "Host=127.0.0.1;Port=5432;Database=ef_design;Username=postgres;Password=postgres";
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(
                Path.Combine(
                    Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName,
                    $"host{Path.DirectorySeparatorChar}EcomMicroService.Cms.HttpApi.Host"
                )
            )
            .AddJsonFile("appsettings.json", false);

        return builder.Build();
    }
}
