using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalSupplies.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BackfillPurchaseOrderStatusHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO "PurchaseOrderStatusHistories"
                    ("PurchaseOrderId", "Status", "ChangedDate", "ChangedBy", "Notes")
                SELECT
                    po."PurchaseOrderId",
                    po."Status",
                    po."OrderDate",
                    po."CreatedBy",
                    'Initial status recorded during history migration.'
                FROM "PurchaseOrders" po
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM "PurchaseOrderStatusHistories" h
                    WHERE h."PurchaseOrderId" = po."PurchaseOrderId"
                );
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM "PurchaseOrderStatusHistories"
                WHERE "Notes" = 'Initial status recorded during history migration.';
            """);
        }
    }
}
