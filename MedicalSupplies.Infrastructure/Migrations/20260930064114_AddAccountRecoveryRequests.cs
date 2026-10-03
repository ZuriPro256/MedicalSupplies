using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MedicalSupplies.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountRecoveryRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountRecoveryRequests",
                columns: table => new
                {
                    AccountRecoveryRequestId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    RecoveryType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SubmittedEmail = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    SubmittedPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NewContactPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    VerificationMethod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VerificationNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReviewedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ReviewedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountRecoveryRequests", x => x.AccountRecoveryRequestId);
                    table.ForeignKey(
                        name: "FK_AccountRecoveryRequests_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountRecoveryRequests_CustomerId",
                table: "AccountRecoveryRequests",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountRecoveryRequests_Status",
                table: "AccountRecoveryRequests",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountRecoveryRequests");
        }
    }
}
