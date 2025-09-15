using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FridgeManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class modified_tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RequestedDate",
                table: "PurchaseRequests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "PurchaseRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScrapDate",
                table: "Fridges",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Fridges",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "Fridges",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Allocations",
                columns: table => new
                {
                    AllocationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    FridgeId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Allocations", x => x.AllocationId);
                    table.ForeignKey(
                        name: "FK_Allocations_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Allocations_Fridges_FridgeId",
                        column: x => x.FridgeId,
                        principalTable: "Fridges",
                        principalColumn: "FridgeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Fridges_SupplierId",
                table: "Fridges",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Allocations_CustomerId",
                table: "Allocations",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Allocations_FridgeId",
                table: "Allocations",
                column: "FridgeId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Fridges_Suppliers_SupplierId",
                table: "Fridges",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "SupplierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Fridges_Suppliers_SupplierId",
                table: "Fridges");

            migrationBuilder.DropTable(
                name: "Allocations");

            migrationBuilder.DropIndex(
                name: "IX_Fridges_SupplierId",
                table: "Fridges");

            migrationBuilder.DropColumn(
                name: "RequestedDate",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "ScrapDate",
                table: "Fridges");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Fridges");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "Fridges");
        }
    }
}
