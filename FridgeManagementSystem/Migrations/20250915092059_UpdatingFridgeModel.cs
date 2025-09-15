using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FridgeManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdatingFridgeModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PurchaseDate",
                table: "Fridges",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "Fridges",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Fridges",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageFile",
                table: "Fridges",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Fridges",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Fridges",
                type: "decimal(16,2)",
                precision: 16,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Fridges");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Fridges");

            migrationBuilder.DropColumn(
                name: "ImageFile",
                table: "Fridges");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Fridges");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Fridges");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Fridges",
                newName: "PurchaseDate");
        }
    }
}
