using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MedicalSupplies.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseOrderReturns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PurchaseOrderReturns",
                columns: table => new
                {
                    PurchaseOrderReturnId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PurchaseOrderId = table.Column<int>(type: "integer", nullable: false),
                    ReturnNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    RecordedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderReturns", x => x.PurchaseOrderReturnId);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderReturns_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "PurchaseOrderId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderReturnDetails",
                columns: table => new
                {
                    PurchaseOrderReturnDetailId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PurchaseOrderReturnId = table.Column<int>(type: "integer", nullable: false),
                    PurchaseOrderDetailId = table.Column<int>(type: "integer", nullable: false),
                    BatchId = table.Column<int>(type: "integer", nullable: false),
                    QuantityReturned = table.Column<int>(type: "integer", nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderReturnDetails", x => x.PurchaseOrderReturnDetailId);
                    table.CheckConstraint("CK_PurchaseOrderReturnDetail_QuantityReturned", "\"QuantityReturned\" > 0");
                    table.ForeignKey(
                        name: "FK_PurchaseOrderReturnDetails_InventoryBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "InventoryBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderReturnDetails_PurchaseOrderDetails_PurchaseOrd~",
                        column: x => x.PurchaseOrderDetailId,
                        principalTable: "PurchaseOrderDetails",
                        principalColumn: "PurchaseOrderDetailId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderReturnDetails_PurchaseOrderReturns_PurchaseOrd~",
                        column: x => x.PurchaseOrderReturnId,
                        principalTable: "PurchaseOrderReturns",
                        principalColumn: "PurchaseOrderReturnId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderReturnDetails_BatchId",
                table: "PurchaseOrderReturnDetails",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderReturnDetails_PurchaseOrderDetailId",
                table: "PurchaseOrderReturnDetails",
                column: "PurchaseOrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderReturnDetails_PurchaseOrderReturnId",
                table: "PurchaseOrderReturnDetails",
                column: "PurchaseOrderReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderReturns_PurchaseOrderId",
                table: "PurchaseOrderReturns",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderReturns_ReturnNumber",
                table: "PurchaseOrderReturns",
                column: "ReturnNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchaseOrderReturnDetails");

            migrationBuilder.DropTable(
                name: "PurchaseOrderReturns");
        }
    }
}
