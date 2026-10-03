using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalSupplies.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseOrderAmountPaid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AmountPaid",
                table: "PurchaseOrders",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddCheckConstraint(
                name: "CK_PurchaseOrders_AmountPaid_Valid",
                table: "PurchaseOrders",
                sql: "\"AmountPaid\" >= 0 AND \"AmountPaid\" <= \"TotalAmount\"");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_PurchaseOrders_AmountPaid_Valid",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "AmountPaid",
                table: "PurchaseOrders");
        }
    }
}
