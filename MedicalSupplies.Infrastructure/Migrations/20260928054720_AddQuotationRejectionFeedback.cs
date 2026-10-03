using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalSupplies.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuotationRejectionFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CustomerExpectedPrice",
                table: "Quotations",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerRejectionComment",
                table: "Quotations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedDate",
                table: "Quotations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RejectionReason",
                table: "Quotations",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerExpectedPrice",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "CustomerRejectionComment",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "RejectedDate",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Quotations");
        }
    }
}
