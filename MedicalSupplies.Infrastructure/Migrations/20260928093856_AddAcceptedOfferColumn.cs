using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalSupplies.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAcceptedOfferColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AcceptedOfferId",
                table: "Quotations",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_AcceptedOfferId",
                table: "Quotations",
                column: "AcceptedOfferId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quotations_QuotationOffers_AcceptedOfferId",
                table: "Quotations",
                column: "AcceptedOfferId",
                principalTable: "QuotationOffers",
                principalColumn: "QuotationOfferId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quotations_QuotationOffers_AcceptedOfferId",
                table: "Quotations");

            migrationBuilder.DropIndex(
                name: "IX_Quotations_AcceptedOfferId",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "AcceptedOfferId",
                table: "Quotations");
        }
    }
}
