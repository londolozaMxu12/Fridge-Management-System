using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FridgeManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderIdToAllocationModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.AddColumn<int>(
                name: "OrderId",
                table: "Allocations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Allocations_OrderId",
                table: "Allocations",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Allocations_Orders_OrderId",
                table: "Allocations",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Allocations_Orders_OrderId",
                table: "Allocations");


            migrationBuilder.DropIndex(
                name: "IX_Allocations_OrderId",
                table: "Allocations");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "Allocations");

        }
    }
}
