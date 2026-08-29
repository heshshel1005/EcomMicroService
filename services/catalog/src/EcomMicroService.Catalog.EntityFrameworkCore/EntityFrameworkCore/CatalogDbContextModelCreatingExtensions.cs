using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace EcomMicroService.Catalog.EntityFrameworkCore;

public static class CatalogDbContextModelCreatingExtensions
{
    public static void ConfigureCatalog(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<Category>(b =>
        {
            b.ToTable(CatalogDbProperties.DbTablePrefix + "Categories", CatalogDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).HasMaxLength(CatalogConsts.Catalog.CategoryMaxNameLength);
            b.Property(x => x.Slug).HasMaxLength(CatalogConsts.Catalog.CategoryMaxSlugLength);
            b.HasIndex(x => x.ParentId);
            b.HasIndex(x => new { x.TenantId, x.Slug }).IsUnique();
        });

        builder.Entity<CategoryTranslation>(b =>
        {
            b.ToTable(CatalogDbProperties.DbTablePrefix + "CategoryTranslations", CatalogDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CategoryId, x.Language }).IsUnique();
            b.HasOne(x => x.Category)
                .WithMany(x => x.Translations)
                .HasForeignKey(x => x.CategoryId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProductType>(b =>
        {
            b.ToTable(CatalogDbProperties.DbTablePrefix + "ProductTypes", CatalogDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Code).HasMaxLength(CatalogConsts.Catalog.ProductTypeMaxCodeLength);
            b.Property(x => x.Name).HasMaxLength(CatalogConsts.Catalog.ProductTypeMaxNameLength);
            b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.Name });
        });

        builder.Entity<ProductTypeTranslation>(b =>
        {
            b.ToTable(CatalogDbProperties.DbTablePrefix + "ProductTypeTranslations", CatalogDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Language).HasMaxLength(CatalogConsts.Catalog.TranslationLanguageMaxLength);
            b.Property(x => x.Name).HasMaxLength(CatalogConsts.Catalog.ProductTypeMaxNameLength);
            b.HasIndex(x => new { x.ProductTypeId, x.Language }).IsUnique();
            b.HasOne(x => x.ProductType)
                .WithMany(x => x.Translations)
                .HasForeignKey(x => x.ProductTypeId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AttributeDefinition>(b =>
        {
            b.ToTable(CatalogDbProperties.DbTablePrefix + "AttributeDefinitions", CatalogDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Key).HasMaxLength(CatalogConsts.Catalog.ProductTypeRuleMaxConditionAttributeKeyLength);
            b.Property(x => x.AllowedValuesJson).HasMaxLength(CatalogConsts.Catalog.ProductMaxDescriptionLength);
            b.Property(x => x.RegexPattern).HasMaxLength(CatalogConsts.Catalog.ProductMaxDescriptionLength);
            b.HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
        });

        builder.Entity<AttributeDefinitionTranslation>(b =>
        {
            b.ToTable(CatalogDbProperties.DbTablePrefix + "AttributeDefinitionTranslations", CatalogDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Language).HasMaxLength(CatalogConsts.Catalog.TranslationLanguageMaxLength);
            b.Property(x => x.DisplayName).HasMaxLength(CatalogConsts.Catalog.AttributeDefinitionTranslationDisplayNameMaxLength);
            b.Property(x => x.Description).HasMaxLength(CatalogConsts.Catalog.AttributeDefinitionTranslationDescriptionMaxLength);
            b.HasIndex(x => new { x.AttributeDefinitionId, x.Language }).IsUnique();
            b.HasOne(x => x.AttributeDefinition)
                .WithMany(x => x.Translations)
                .HasForeignKey(x => x.AttributeDefinitionId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AttributeOption>(b =>
        {
            b.ToTable(CatalogDbProperties.DbTablePrefix + "AttributeOptions", CatalogDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Value).HasMaxLength(CatalogConsts.Catalog.AttributeOptionValueMaxLength);
            b.HasIndex(x => new { x.TenantId, x.AttributeDefinitionId, x.Value }).IsUnique();
            b.HasIndex(x => x.AttributeDefinitionId);
            b.HasOne(x => x.AttributeDefinition)
                .WithMany(x => x.Options)
                .HasForeignKey(x => x.AttributeDefinitionId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AttributeOptionTranslation>(b =>
        {
            b.ToTable(CatalogDbProperties.DbTablePrefix + "AttributeOptionTranslations", CatalogDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Language).HasMaxLength(CatalogConsts.Catalog.TranslationLanguageMaxLength);
            b.Property(x => x.DisplayName).HasMaxLength(CatalogConsts.Catalog.AttributeOptionTranslationDisplayNameMaxLength);
            b.HasIndex(x => new { x.TenantId, x.AttributeOptionId, x.Language }).IsUnique();
            b.HasOne(x => x.AttributeOption)
                .WithMany(x => x.Translations)
                .HasForeignKey(x => x.AttributeOptionId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProductTypeAttributeRule>(b =>
        {
            b.ToTable(CatalogDbProperties.DbTablePrefix + "ProductTypeAttributeRules", CatalogDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ConditionalAttributeKey).HasMaxLength(CatalogConsts.Catalog.ProductTypeRuleMaxConditionAttributeKeyLength);
            b.Property(x => x.ConditionalExpectedValue).HasMaxLength(CatalogConsts.Catalog.ProductTypeRuleMaxConditionExpectedValueLength);
            b.HasIndex(x => x.ProductTypeId);
            b.HasIndex(x => x.AttributeDefinitionId);
            b.HasIndex(x => new { x.TenantId, x.ProductTypeId, x.AttributeDefinitionId }).IsUnique();
            b.HasOne<ProductType>()
                .WithMany()
                .HasForeignKey(x => x.ProductTypeId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne<AttributeDefinition>()
                .WithMany()
                .HasForeignKey(x => x.AttributeDefinitionId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProductAttribute>(b =>
        {
            b.ToTable(CatalogDbProperties.DbTablePrefix + "ProductAttributes", CatalogDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).HasMaxLength(CatalogConsts.Catalog.ProductAttributeMaxNameLength);
            b.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
        });

        builder.Entity<Brand>(b =>
        {
            b.ToTable(CatalogDbProperties.DbTablePrefix + "Brands", CatalogDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).HasMaxLength(CatalogConsts.Catalog.BrandMaxNameLength);
            b.Property(x => x.Slug).HasMaxLength(CatalogConsts.Catalog.BrandMaxSlugLength);
            b.Property(x => x.Description).HasMaxLength(CatalogConsts.Catalog.BrandMaxDescriptionLength);
            b.HasIndex(x => x.Name);
        });

        builder.Entity<BrandTranslation>(b =>
        {
            b.ToTable(CatalogDbProperties.DbTablePrefix + "BrandTranslations", CatalogDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.BrandId, x.Language }).IsUnique();
            b.HasOne(x => x.Brand)
                .WithMany(x => x.Translations)
                .HasForeignKey(x => x.BrandId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<BrandModel>(b =>
        {
            b.ToTable(CatalogDbProperties.DbTablePrefix + "BrandModels", CatalogDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).HasMaxLength(CatalogConsts.Catalog.BrandModelMaxNameLength);
            b.Property(x => x.Code).HasMaxLength(CatalogConsts.Catalog.BrandModelMaxCodeLength);
            b.HasIndex(x => x.BrandId);
            b.HasIndex(x => new { x.TenantId, x.BrandId, x.Name });
            b.HasOne(x => x.Brand).WithMany(x => x.Models).HasForeignKey(x => x.BrandId).IsRequired();
        });

        builder.Entity<BrandModelTranslation>(b =>
        {
            b.ToTable(CatalogDbProperties.DbTablePrefix + "BrandModelTranslations", CatalogDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.BrandModelId, x.Language }).IsUnique();
            b.HasOne(x => x.BrandModel)
                .WithMany(x => x.Translations)
                .HasForeignKey(x => x.BrandModelId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Product>(b =>
        {
            b.ToTable(CatalogDbProperties.DbTablePrefix + "Products", CatalogDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ProductNumber).HasMaxLength(CatalogConsts.Catalog.ProductMaxProductNumberLength);
            b.Property(x => x.Name).HasMaxLength(CatalogConsts.Catalog.ProductMaxNameLength);
            b.Property(x => x.Description).HasMaxLength(CatalogConsts.Catalog.ProductMaxDescriptionLength);
            b.HasIndex(x => new { x.TenantId, x.ProductNumber }).IsUnique();
            b.HasIndex(x => x.CategoryId);
            b.HasIndex(x => x.BrandId);
            b.HasIndex(x => x.ModelId);
            b.HasOne(x => x.Brand).WithMany().HasForeignKey(x => x.BrandId).IsRequired();
            b.HasOne(x => x.Model).WithMany().HasForeignKey(x => x.ModelId).IsRequired(false);
        });

        builder.Entity<ProductTranslation>(b =>
        {
            b.ToTable(CatalogDbProperties.DbTablePrefix + "ProductTranslations", CatalogDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.ProductId, x.Language }).IsUnique();
            b.HasOne(x => x.Product)
                .WithMany(x => x.Translations)
                .HasForeignKey(x => x.ProductId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProductVariant>(b =>
        {
            b.ToTable(CatalogDbProperties.DbTablePrefix + "ProductVariants", CatalogDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Sku).HasMaxLength(CatalogConsts.Catalog.ProductVariantMaxSkuLength);
            b.Property(x => x.DynamicAttributesJson).HasMaxLength(CatalogConsts.Catalog.ProductMaxDescriptionLength);
            b.HasIndex(x => new { x.TenantId, x.Sku }).IsUnique();
            b.HasIndex(x => x.ProductId);
        });

        builder.Entity<ProductVariantAttribute>(b =>
        {
            b.ToTable(CatalogDbProperties.DbTablePrefix + "ProductVariantAttributes", CatalogDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Value).HasMaxLength(CatalogConsts.Catalog.ProductVariantAttributeMaxValueLength);
            b.HasIndex(x => x.ProductVariantId);
            b.HasIndex(x => x.ProductAttributeId);
        });

        builder.Entity<ProductMedia>(b =>
        {
            b.ToTable(CatalogDbProperties.DbTablePrefix + "ProductMedia", CatalogDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.FilePathOrBlobKey).HasMaxLength(CatalogConsts.Catalog.ProductMediaMaxFilePathLength);
            b.Property(x => x.AltText).HasMaxLength(CatalogConsts.Catalog.ProductMediaMaxAltTextLength);
            b.HasIndex(x => x.ProductId);
        });

        builder.Entity<Inventory>(b =>
        {
            b.ToTable(CatalogDbProperties.DbTablePrefix + "Inventories", CatalogDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.TenantId, x.ProductVariantId }).IsUnique();
        });

        builder.Entity<ProductReview>(b =>
        {
            b.ToTable(CatalogDbProperties.DbTablePrefix + "ProductReviews", CatalogDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ReviewText).HasMaxLength(CatalogConsts.Catalog.ProductReviewMaxReviewTextLength);
            b.HasIndex(x => x.ProductId);
            b.HasIndex(x => x.UserId);
            b.HasIndex(x => x.Status);
            b.HasIndex(x => new { x.TenantId, x.ProductId, x.UserId }).IsUnique();
        });
    }
}
