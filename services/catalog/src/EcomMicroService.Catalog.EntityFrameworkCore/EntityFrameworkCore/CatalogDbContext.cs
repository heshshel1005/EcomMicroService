using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace EcomMicroService.Catalog.EntityFrameworkCore;

[ConnectionStringName(EcomMicroServiceNames.CatalogDb)]
public class CatalogDbContext(DbContextOptions<CatalogDbContext> options)
    : AbpDbContext<CatalogDbContext>(options),
        ICatalogDbContext
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<CategoryTranslation> CategoryTranslations { get; set; }
    public DbSet<ProductType> ProductTypes { get; set; }
    public DbSet<ProductTypeTranslation> ProductTypeTranslations { get; set; }
    public DbSet<AttributeDefinition> AttributeDefinitions { get; set; }
    public DbSet<AttributeDefinitionTranslation> AttributeDefinitionTranslations { get; set; }
    public DbSet<AttributeOption> AttributeOptions { get; set; }
    public DbSet<AttributeOptionTranslation> AttributeOptionTranslations { get; set; }
    public DbSet<ProductTypeAttributeRule> ProductTypeAttributeRules { get; set; }
    public DbSet<ProductAttribute> ProductAttributes { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<BrandTranslation> BrandTranslations { get; set; }
    public DbSet<BrandModel> BrandModels { get; set; }
    public DbSet<BrandModelTranslation> BrandModelTranslations { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductTranslation> ProductTranslations { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<ProductVariantAttribute> ProductVariantAttributes { get; set; }
    public DbSet<ProductMedia> ProductMedia { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<ProductReview> ProductReviews { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ConfigureCatalog();
    }
}
