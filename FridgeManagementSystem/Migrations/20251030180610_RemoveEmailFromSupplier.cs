using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FridgeManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEmailFromSupplier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Suppliers");

            

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

        }
    }
}
