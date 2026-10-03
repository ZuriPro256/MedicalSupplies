using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalSupplies.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierPaymentReversalStateConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_SupplierPayments_Reversal_State",
                table: "SupplierPayments",
                sql: "(\n    \"IsReversed\" = FALSE\n    AND \"ReversedDate\" IS NULL\n    AND \"ReversedBy\" IS NULL\n    AND \"ReversalReason\" IS NULL\n)\nOR\n(\n    \"IsReversed\" = TRUE\n    AND \"ReversedDate\" IS NOT NULL\n    AND \"ReversedBy\" IS NOT NULL\n    AND btrim(\"ReversedBy\") <> ''\n    AND \"ReversalReason\" IS NOT NULL\n    AND btrim(\"ReversalReason\") <> ''\n)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_SupplierPayments_Reversal_State",
                table: "SupplierPayments");
        }
    }
}
