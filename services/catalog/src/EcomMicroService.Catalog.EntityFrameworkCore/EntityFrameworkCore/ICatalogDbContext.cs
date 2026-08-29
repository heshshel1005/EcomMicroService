using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace EcomMicroService.Catalog.EntityFrameworkCore;

[ConnectionStringName(EcomMicroServiceNames.CatalogDb)]
public interface ICatalogDbContext : IEfCoreDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<CategoryTranslation> CategoryTranslations { get; }
    DbSet<ProductType> ProductTypes { get; }
    DbSet<ProductTypeTranslation> ProductTypeTranslations { get; }
    DbSet<AttributeDefinition> AttributeDefinitions { get; }
    DbSet<AttributeDefinitionTranslation> AttributeDefinitionTranslations { get; }
    DbSet<AttributeOption> AttributeOptions { get; }
    DbSet<AttributeOptionTranslation> AttributeOptionTranslations { get; }
    DbSet<ProductTypeAttributeRule> ProductTypeAttributeRules { get; }
    DbSet<ProductAttribute> ProductAttributes { get; }
    DbSet<Brand> Brands { get; }
    DbSet<BrandTranslation> BrandTranslations { get; }
    DbSet<BrandModel> BrandModels { get; }
    DbSet<BrandModelTranslation> BrandModelTranslations { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductTranslation> ProductTranslations { get; }
    DbSet<ProductVariant> ProductVariants { get; }
    DbSet<ProductVariantAttribute> ProductVariantAttributes { get; }
    DbSet<ProductMedia> ProductMedia { get; }
    DbSet<Inventory> Inventories { get; }
    DbSet<ProductReview> ProductReviews { get; }
}
