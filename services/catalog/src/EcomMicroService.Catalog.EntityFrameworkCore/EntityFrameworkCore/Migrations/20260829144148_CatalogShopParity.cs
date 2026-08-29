using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcomMicroService.Catalog.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class CatalogShopParity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CatalogProducts_CatalogCategories_CategoryId",
                table: "CatalogProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_CatalogProducts_CatalogModels_ModelId",
                table: "CatalogProducts");

            migrationBuilder.DropTable(
                name: "CatalogModels");

            migrationBuilder.DropIndex(
                name: "IX_CatalogProducts_Sku",
                table: "CatalogProducts");

            migrationBuilder.DropIndex(
                name: "IX_CatalogCategories_Code",
                table: "CatalogCategories");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "CatalogProducts");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CatalogProducts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CatalogProducts");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "CatalogProducts");

            migrationBuilder.DropColumn(
                name: "Sku",
                table: "CatalogProducts");

            migrationBuilder.DropColumn(
                name: "StockQuantity",
                table: "CatalogProducts");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "CatalogCategories");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "CatalogCategories");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "CatalogCategories");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CatalogCategories");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CatalogCategories");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "CatalogBrands");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "CatalogBrands");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CatalogBrands");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "CatalogBrands");

            migrationBuilder.RenameColumn(
                name: "IsFeatured",
                table: "CatalogProducts",
                newName: "IsPublished");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "CatalogProducts",
                newName: "DynamicAttributesJson");

            migrationBuilder.RenameColumn(
                name: "DeleterId",
                table: "CatalogProducts",
                newName: "ProductTypeId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CatalogProducts",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "CatalogProducts",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CategoryId",
                table: "CatalogProducts",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "ProductNumber",
                table: "CatalogProducts",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CatalogCategories",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "CatalogCategories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "CatalogCategories",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CatalogBrands",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CatalogBrands",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "CatalogBrands",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CatalogAttributeDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DataType = table.Column<int>(type: "integer", nullable: false),
                    AllowedValuesJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RegexPattern = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    MinValue = table.Column<decimal>(type: "numeric", nullable: true),
                    MaxValue = table.Column<decimal>(type: "numeric", nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    IsRecommended = table.Column<bool>(type: "boolean", nullable: false),
                    GovernanceStatus = table.Column<int>(type: "integer", nullable: false),
                    PublishedVersion = table.Column<int>(type: "integer", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogAttributeDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogBrandModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    BrandId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogBrandModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogBrandModels_CatalogBrands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "CatalogBrands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CatalogBrandTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BrandId = table.Column<Guid>(type: "uuid", nullable: false),
                    Language = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogBrandTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogBrandTranslations_CatalogBrands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "CatalogBrands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CatalogCategoryTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Language = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogCategoryTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogCategoryTranslations_CatalogCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "CatalogCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CatalogInventories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Reserved = table.Column<int>(type: "integer", nullable: false),
                    LowStockThreshold = table.Column<int>(type: "integer", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogInventories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogProductAttributes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogProductAttributes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogProductMedia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    MediaType = table.Column<int>(type: "integer", nullable: false),
                    FilePathOrBlobKey = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    AltText = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogProductMedia", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogProductReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    ReviewText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogProductReviews", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogProductTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Language = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogProductTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogProductTranslations_CatalogProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "CatalogProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CatalogProductTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogProductTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogProductVariantAttributes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductAttributeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogProductVariantAttributes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogProductVariants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sku = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Price = table.Column<decimal>(type: "numeric", nullable: true),
                    DynamicAttributesJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogProductVariants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogAttributeDefinitionTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    AttributeDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogAttributeDefinitionTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogAttributeDefinitionTranslations_CatalogAttributeDefi~",
                        column: x => x.AttributeDefinitionId,
                        principalTable: "CatalogAttributeDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CatalogAttributeOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    AttributeDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogAttributeOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogAttributeOptions_CatalogAttributeDefinitions_Attribu~",
                        column: x => x.AttributeDefinitionId,
                        principalTable: "CatalogAttributeDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CatalogBrandModelTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BrandModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    Language = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogBrandModelTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogBrandModelTranslations_CatalogBrandModels_BrandModel~",
                        column: x => x.BrandModelId,
                        principalTable: "CatalogBrandModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CatalogProductTypeAttributeRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttributeDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    ConditionalAttributeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ConditionalOperator = table.Column<int>(type: "integer", nullable: true),
                    ConditionalExpectedValue = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogProductTypeAttributeRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogProductTypeAttributeRules_CatalogAttributeDefinition~",
                        column: x => x.AttributeDefinitionId,
                        principalTable: "CatalogAttributeDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CatalogProductTypeAttributeRules_CatalogProductTypes_Produc~",
                        column: x => x.ProductTypeId,
                        principalTable: "CatalogProductTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CatalogProductTypeTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogProductTypeTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogProductTypeTranslations_CatalogProductTypes_ProductT~",
                        column: x => x.ProductTypeId,
                        principalTable: "CatalogProductTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CatalogAttributeOptionTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    AttributeOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogAttributeOptionTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogAttributeOptionTranslations_CatalogAttributeOptions_~",
                        column: x => x.AttributeOptionId,
                        principalTable: "CatalogAttributeOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogProducts_TenantId_ProductNumber",
                table: "CatalogProducts",
                columns: new[] { "TenantId", "ProductNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogCategories_ParentId",
                table: "CatalogCategories",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogCategories_TenantId_Slug",
                table: "CatalogCategories",
                columns: new[] { "TenantId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogAttributeDefinitions_TenantId_Key",
                table: "CatalogAttributeDefinitions",
                columns: new[] { "TenantId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogAttributeDefinitionTranslations_AttributeDefinitionI~",
                table: "CatalogAttributeDefinitionTranslations",
                columns: new[] { "AttributeDefinitionId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogAttributeOptions_AttributeDefinitionId",
                table: "CatalogAttributeOptions",
                column: "AttributeDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogAttributeOptions_TenantId_AttributeDefinitionId_Value",
                table: "CatalogAttributeOptions",
                columns: new[] { "TenantId", "AttributeDefinitionId", "Value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogAttributeOptionTranslations_AttributeOptionId",
                table: "CatalogAttributeOptionTranslations",
                column: "AttributeOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogAttributeOptionTranslations_TenantId_AttributeOption~",
                table: "CatalogAttributeOptionTranslations",
                columns: new[] { "TenantId", "AttributeOptionId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogBrandModels_BrandId",
                table: "CatalogBrandModels",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogBrandModels_TenantId_BrandId_Name",
                table: "CatalogBrandModels",
                columns: new[] { "TenantId", "BrandId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogBrandModelTranslations_BrandModelId_Language",
                table: "CatalogBrandModelTranslations",
                columns: new[] { "BrandModelId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogBrandTranslations_BrandId_Language",
                table: "CatalogBrandTranslations",
                columns: new[] { "BrandId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogCategoryTranslations_CategoryId_Language",
                table: "CatalogCategoryTranslations",
                columns: new[] { "CategoryId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogInventories_TenantId_ProductVariantId",
                table: "CatalogInventories",
                columns: new[] { "TenantId", "ProductVariantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogProductAttributes_TenantId_Name",
                table: "CatalogProductAttributes",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogProductMedia_ProductId",
                table: "CatalogProductMedia",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogProductReviews_ProductId",
                table: "CatalogProductReviews",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogProductReviews_Status",
                table: "CatalogProductReviews",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogProductReviews_TenantId_ProductId_UserId",
                table: "CatalogProductReviews",
                columns: new[] { "TenantId", "ProductId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogProductReviews_UserId",
                table: "CatalogProductReviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogProductTranslations_ProductId_Language",
                table: "CatalogProductTranslations",
                columns: new[] { "ProductId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogProductTypeAttributeRules_AttributeDefinitionId",
                table: "CatalogProductTypeAttributeRules",
                column: "AttributeDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogProductTypeAttributeRules_ProductTypeId",
                table: "CatalogProductTypeAttributeRules",
                column: "ProductTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogProductTypeAttributeRules_TenantId_ProductTypeId_Att~",
                table: "CatalogProductTypeAttributeRules",
                columns: new[] { "TenantId", "ProductTypeId", "AttributeDefinitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogProductTypes_TenantId_Code",
                table: "CatalogProductTypes",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogProductTypes_TenantId_Name",
                table: "CatalogProductTypes",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogProductTypeTranslations_ProductTypeId_Language",
                table: "CatalogProductTypeTranslations",
                columns: new[] { "ProductTypeId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogProductVariantAttributes_ProductAttributeId",
                table: "CatalogProductVariantAttributes",
                column: "ProductAttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogProductVariantAttributes_ProductVariantId",
                table: "CatalogProductVariantAttributes",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogProductVariants_ProductId",
                table: "CatalogProductVariants",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogProductVariants_TenantId_Sku",
                table: "CatalogProductVariants",
                columns: new[] { "TenantId", "Sku" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogProducts_CatalogBrandModels_ModelId",
                table: "CatalogProducts",
                column: "ModelId",
                principalTable: "CatalogBrandModels",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CatalogProducts_CatalogBrandModels_ModelId",
                table: "CatalogProducts");

            migrationBuilder.DropTable(
                name: "CatalogAttributeDefinitionTranslations");

            migrationBuilder.DropTable(
                name: "CatalogAttributeOptionTranslations");

            migrationBuilder.DropTable(
                name: "CatalogBrandModelTranslations");

            migrationBuilder.DropTable(
                name: "CatalogBrandTranslations");

            migrationBuilder.DropTable(
                name: "CatalogCategoryTranslations");

            migrationBuilder.DropTable(
                name: "CatalogInventories");

            migrationBuilder.DropTable(
                name: "CatalogProductAttributes");

            migrationBuilder.DropTable(
                name: "CatalogProductMedia");

            migrationBuilder.DropTable(
                name: "CatalogProductReviews");

            migrationBuilder.DropTable(
                name: "CatalogProductTranslations");

            migrationBuilder.DropTable(
                name: "CatalogProductTypeAttributeRules");

            migrationBuilder.DropTable(
                name: "CatalogProductTypeTranslations");

            migrationBuilder.DropTable(
                name: "CatalogProductVariantAttributes");

            migrationBuilder.DropTable(
                name: "CatalogProductVariants");

            migrationBuilder.DropTable(
                name: "CatalogAttributeOptions");

            migrationBuilder.DropTable(
                name: "CatalogBrandModels");

            migrationBuilder.DropTable(
                name: "CatalogProductTypes");

            migrationBuilder.DropTable(
                name: "CatalogAttributeDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_CatalogProducts_TenantId_ProductNumber",
                table: "CatalogProducts");

            migrationBuilder.DropIndex(
                name: "IX_CatalogCategories_ParentId",
                table: "CatalogCategories");

            migrationBuilder.DropIndex(
                name: "IX_CatalogCategories_TenantId_Slug",
                table: "CatalogCategories");

            migrationBuilder.DropColumn(
                name: "ProductNumber",
                table: "CatalogProducts");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "CatalogCategories");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "CatalogCategories");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "CatalogBrands");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "CatalogBrands");

            migrationBuilder.RenameColumn(
                name: "ProductTypeId",
                table: "CatalogProducts",
                newName: "DeleterId");

            migrationBuilder.RenameColumn(
                name: "IsPublished",
                table: "CatalogProducts",
                newName: "IsFeatured");

            migrationBuilder.RenameColumn(
                name: "DynamicAttributesJson",
                table: "CatalogProducts",
                newName: "ImageUrl");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CatalogProducts",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "CatalogProducts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CategoryId",
                table: "CatalogProducts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "CatalogProducts",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CatalogProducts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CatalogProducts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "CatalogProducts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Sku",
                table: "CatalogProducts",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                table: "CatalogProducts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CatalogCategories",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "CatalogCategories",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "CatalogCategories",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "CatalogCategories",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CatalogCategories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CatalogCategories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CatalogBrands",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "CatalogBrands",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "CatalogBrands",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CatalogBrands",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "CatalogBrands",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CatalogModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BrandId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ReleaseYear = table.Column<int>(type: "integer", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogModels_CatalogBrands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "CatalogBrands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogProducts_Sku",
                table: "CatalogProducts",
                column: "Sku");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogCategories_Code",
                table: "CatalogCategories",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogModels_BrandId",
                table: "CatalogModels",
                column: "BrandId");

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogProducts_CatalogCategories_CategoryId",
                table: "CatalogProducts",
                column: "CategoryId",
                principalTable: "CatalogCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogProducts_CatalogModels_ModelId",
                table: "CatalogProducts",
                column: "ModelId",
                principalTable: "CatalogModels",
                principalColumn: "Id");
        }
    }
}
