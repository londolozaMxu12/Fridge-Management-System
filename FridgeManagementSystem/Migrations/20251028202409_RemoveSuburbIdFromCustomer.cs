using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FridgeManagementSystem.Migrations
{
    public partial class RemoveSuburbIdFromCustomer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First drop the foreign key constraint for SuburbId1
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Suburbs_SuburbId1",
                table: "Customers");

            // Drop the index for SuburbId1
            migrationBuilder.DropIndex(
                name: "IX_Customers_SuburbId1",
                table: "Customers");

            // Remove both SuburbId columns
            migrationBuilder.DropColumn(
                name: "SuburbId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "SuburbId1",
                table: "Customers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Add the columns back for rollback
            migrationBuilder.AddColumn<string>(
                name: "SuburbId",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SuburbId1",
                table: "Customers",
                type: "int",
                nullable: true);

            // Recreate the index and foreign key
            migrationBuilder.CreateIndex(
                name: "IX_Customers_SuburbId1",
                table: "Customers",
                column: "SuburbId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Suburbs_SuburbId1",
                table: "Customers",
                column: "SuburbId1",
                principalTable: "Suburbs",
                principalColumn: "SuburbId");
        }
    }
}