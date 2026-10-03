using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalSupplies.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuotationOfferDeliveryCost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryCost",
                table: "QuotationOffers",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryCost",
                table: "QuotationOffers");
        }
    }
}
