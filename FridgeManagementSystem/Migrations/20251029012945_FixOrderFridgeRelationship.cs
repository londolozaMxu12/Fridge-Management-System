using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FridgeManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class FixOrderFridgeRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove the foreign key constraint from Orders to Fridges
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Fridges_FridgeId",
                table: "Orders");

            // Remove the index on FridgeId in Orders table
            migrationBuilder.DropIndex(
                name: "IX_Orders_FridgeId",
                table: "Orders");

            // Remove the FridgeId column from Orders table
            migrationBuilder.DropColumn(
                name: "FridgeId",
                table: "Orders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Add the FridgeId column back to Orders table
            migrationBuilder.AddColumn<int>(
                name: "FridgeId",
                table: "Orders",
                type: "int",
                nullable: true);

            // Recreate the index on FridgeId
            migrationBuilder.CreateIndex(
                name: "IX_Orders_FridgeId",
                table: "Orders",
                column: "FridgeId",
                unique: true,
                filter: "[FridgeId] IS NOT NULL");

            // Recreate the foreign key constraint
            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Fridges_FridgeId",
                table: "Orders",
                column: "FridgeId",
                principalTable: "Fridges",
                principalColumn: "FridgeId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}