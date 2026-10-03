using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MedicalSupplies.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuotationOffers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuotationOffers",
                columns: table => new
                {
                    QuotationOfferId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuotationId = table.Column<int>(type: "integer", nullable: false),
                    RevisionNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RespondedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PreparedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    AdminNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ValidUntil = table.Column<DateOnly>(type: "date", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CustomerExpectedPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CustomerRejectionComment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RejectedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OfferNumber = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationOffers", x => x.QuotationOfferId);
                    table.ForeignKey(
                        name: "FK_QuotationOffers_Quotations_QuotationId",
                        column: x => x.QuotationId,
                        principalTable: "Quotations",
                        principalColumn: "QuotationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuotationOfferDetails",
                columns: table => new
                {
                    QuotationOfferDetailId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuotationOfferId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationOfferDetails", x => x.QuotationOfferDetailId);
                    table.ForeignKey(
                        name: "FK_QuotationOfferDetails_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuotationOfferDetails_QuotationOffers_QuotationOfferId",
                        column: x => x.QuotationOfferId,
                        principalTable: "QuotationOffers",
                        principalColumn: "QuotationOfferId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuotationOfferDetails_ProductId",
                table: "QuotationOfferDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationOfferDetails_QuotationOfferId",
                table: "QuotationOfferDetails",
                column: "QuotationOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationOffers_OfferNumber",
                table: "QuotationOffers",
                column: "OfferNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuotationOffers_QuotationId_RevisionNumber",
                table: "QuotationOffers",
                columns: new[] { "QuotationId", "RevisionNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuotationOfferDetails");

            migrationBuilder.DropTable(
                name: "QuotationOffers");
        }
    }
}
