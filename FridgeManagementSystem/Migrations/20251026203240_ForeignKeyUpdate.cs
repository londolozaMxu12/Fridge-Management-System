using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FridgeManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class ForeignKeyUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "ScheduleMaintenances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleMaintenances_CustomerId",
                table: "ScheduleMaintenances",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleMaintenances_Customers_CustomerId",
                table: "ScheduleMaintenances",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleMaintenances_Customers_CustomerId",
                table: "ScheduleMaintenances");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleMaintenances_CustomerId",
                table: "ScheduleMaintenances");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "ScheduleMaintenances");
        }
    }
}
