using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalSupplies.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCatalogueAvailabilityAndPromotion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Availability",
                table: "Products",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "InStock");

            migrationBuilder.AddColumn<DateOnly>(
                name: "SaleEndDate",
                table: "Products",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SalePrice",
                table: "Products",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "SaleStartDate",
                table: "Products",
                type: "date",
                nullable: true);

            // Preserve the catalogue behaviour of existing products.
            // Products that previously depended on batch stock are marked
            // OutOfStock when they currently have no usable stock.
            // Non-batch products retain the previous catalogue behaviour.
            migrationBuilder.Sql("""
                UPDATE "Products" p
                SET "Availability" = 'OutOfStock'
                WHERE p."RequiresBatchTracking" = TRUE
                  AND NOT EXISTS
                  (
                      SELECT 1
                      FROM "InventoryBatches" b
                      WHERE b."ProductId" = p."ProductId"
                        AND b."Status" = 'Active'
                        AND b."QuantityAvailable" > 0
                        AND (
                            b."ExpiryDate" IS NULL
                            OR b."ExpiryDate" >=
                                (CURRENT_TIMESTAMP AT TIME ZONE 'Africa/Kampala')::date
                        )
                  );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Availability",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SaleEndDate",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SalePrice",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SaleStartDate",
                table: "Products");
        }
    }
}
