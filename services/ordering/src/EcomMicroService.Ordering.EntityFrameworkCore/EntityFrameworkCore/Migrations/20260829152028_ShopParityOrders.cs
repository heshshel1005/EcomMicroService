using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcomMicroService.Ordering.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class ShopParityOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderingOrderItems");

            migrationBuilder.DropColumn(
                name: "BuyerId",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "City",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "OrderDate",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "State",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "Street",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "OrderingOrders");

            migrationBuilder.RenameColumn(
                name: "DeleterId",
                table: "OrderingOrders",
                newName: "UserId");

            migrationBuilder.AddColumn<string>(
                name: "BillingCity",
                table: "OrderingOrders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingCountry",
                table: "OrderingOrders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingPostalCode",
                table: "OrderingOrders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingRegion",
                table: "OrderingOrders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "BillingSameAsShipping",
                table: "OrderingOrders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "BillingStreet",
                table: "OrderingOrders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingStreet2",
                table: "OrderingOrders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "OrderingOrders",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactName",
                table: "OrderingOrders",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "OrderingOrders",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CouponId",
                table: "OrderingOrders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "OrderingOrders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ExternalPaymentId",
                table: "OrderingOrders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentGateway",
                table: "OrderingOrders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentStatus",
                table: "OrderingOrders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingAmount",
                table: "OrderingOrders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ShippingCity",
                table: "OrderingOrders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingCountry",
                table: "OrderingOrders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingInstructions",
                table: "OrderingOrders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingMethodCode",
                table: "OrderingOrders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingMethodName",
                table: "OrderingOrders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingPostalCode",
                table: "OrderingOrders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingRegion",
                table: "OrderingOrders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingStreet",
                table: "OrderingOrders",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingStreet2",
                table: "OrderingOrders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SubTotal",
                table: "OrderingOrders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "OrderingOrders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Total",
                table: "OrderingOrders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "OrderingOrderLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Sku = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderingOrderLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderingOrderLines_OrderingOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "OrderingOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderingOrderStatusHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderingOrderStatusHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderingShipments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Carrier = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TrackingNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ShippedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderingShipments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderingOrders_Status",
                table: "OrderingOrders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_OrderingOrders_UserId",
                table: "OrderingOrders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderingOrderLines_OrderId",
                table: "OrderingOrderLines",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderingOrderStatusHistories_OrderId",
                table: "OrderingOrderStatusHistories",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderingShipments_OrderId",
                table: "OrderingShipments",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderingOrderLines");

            migrationBuilder.DropTable(
                name: "OrderingOrderStatusHistories");

            migrationBuilder.DropTable(
                name: "OrderingShipments");

            migrationBuilder.DropIndex(
                name: "IX_OrderingOrders_Status",
                table: "OrderingOrders");

            migrationBuilder.DropIndex(
                name: "IX_OrderingOrders_UserId",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "BillingCity",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "BillingCountry",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "BillingPostalCode",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "BillingRegion",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "BillingSameAsShipping",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "BillingStreet",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "BillingStreet2",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "ContactName",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "CouponId",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "ExternalPaymentId",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "PaymentGateway",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "ShippingAmount",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "ShippingCity",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "ShippingCountry",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "ShippingInstructions",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "ShippingMethodCode",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "ShippingMethodName",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "ShippingPostalCode",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "ShippingRegion",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "ShippingStreet",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "ShippingStreet2",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "SubTotal",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "OrderingOrders");

            migrationBuilder.DropColumn(
                name: "Total",
                table: "OrderingOrders");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "OrderingOrders",
                newName: "DeleterId");

            migrationBuilder.AddColumn<Guid>(
                name: "BuyerId",
                table: "OrderingOrders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "OrderingOrders",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "OrderingOrders",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "OrderingOrders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "OrderDate",
                table: "OrderingOrders",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "OrderingOrders",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "OrderingOrders",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "OrderingOrders",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "OrderingOrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderingOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderingOrderItems_OrderingOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "OrderingOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderingOrderItems_OrderId",
                table: "OrderingOrderItems",
                column: "OrderId");
        }
    }
}
