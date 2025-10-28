using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FridgeManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class FixCustomerRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First, drop all foreign keys that reference Customers
            migrationBuilder.DropForeignKey(
                name: "FK_Allocations_Customers_CustomerId",
                table: "Allocations");

            migrationBuilder.DropForeignKey(
                name: "FK_Faults_Customers_ReportedById",
                table: "Faults");

            migrationBuilder.DropForeignKey(
                name: "FK_FridgeRequests_Customers_CustomerId",
                table: "FridgeRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Fridges_Customers_CustomerId",
                table: "Fridges");

            migrationBuilder.DropForeignKey(
                name: "FK_Quotations_Customers_CustomerId",
                table: "Quotations");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleMaintenances_Customers_CustomerId",
                table: "ScheduleMaintenances");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_AspNetUsers_UserId",
                table: "Customers");

            // Drop indexes
            migrationBuilder.DropIndex(
                name: "IX_Customers_UserId",
                table: "Customers");

            // FIRST: Change all CustomerId columns to string type BEFORE updating data
            migrationBuilder.AlterColumn<string>(
                name: "CustomerId",
                table: "ScheduleMaintenances",
                type: "nvarchar(450)",
                nullable: true,  // Make nullable temporarily
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerId",
                table: "Quotations",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerId",
                table: "Fridges",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerId",
                table: "FridgeRequests",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "ReportedById",
                table: "Faults",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerId",
                table: "Allocations",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            // Create a temporary table to preserve Customer data and mapping
            migrationBuilder.CreateTable(
                name: "TempCustomerData",
                columns: table => new
                {
                    OldId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BusinessName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CustomerType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByFullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempCustomerData", x => x.OldId);
                });

            // Copy data to temporary table
            migrationBuilder.Sql(@"
                INSERT INTO TempCustomerData (OldId, UserId, BusinessName, CustomerType, IsActive, CreatedAt, CreatedByFullName, CreatedById)
                SELECT Id, UserId, BusinessName, CustomerType, IsActive, CreatedAt, CreatedByFullName, CreatedById
                FROM Customers
            ");

            // Update foreign key references in related tables FIRST
            migrationBuilder.Sql(@"
                UPDATE Fridges SET CustomerId = t.UserId
                FROM Fridges f
                INNER JOIN TempCustomerData t ON f.CustomerId = t.OldId
                WHERE f.CustomerId IS NOT NULL
            ");

            migrationBuilder.Sql(@"
                UPDATE Allocations SET CustomerId = t.UserId
                FROM Allocations a
                INNER JOIN TempCustomerData t ON a.CustomerId = t.OldId
                WHERE a.CustomerId IS NOT NULL
            ");

            migrationBuilder.Sql(@"
                UPDATE Faults SET ReportedById = t.UserId
                FROM Faults f
                INNER JOIN TempCustomerData t ON f.ReportedById = t.OldId
                WHERE f.ReportedById IS NOT NULL
            ");

            migrationBuilder.Sql(@"
                UPDATE FridgeRequests SET CustomerId = t.UserId
                FROM FridgeRequests fr
                INNER JOIN TempCustomerData t ON fr.CustomerId = t.OldId
                WHERE fr.CustomerId IS NOT NULL
            ");

            migrationBuilder.Sql(@"
                UPDATE Quotations SET CustomerId = t.UserId
                FROM Quotations q
                INNER JOIN TempCustomerData t ON q.CustomerId = t.OldId
                WHERE q.CustomerId IS NOT NULL
            ");

            migrationBuilder.Sql(@"
                UPDATE ScheduleMaintenances SET CustomerId = t.UserId
                FROM ScheduleMaintenances sm
                INNER JOIN TempCustomerData t ON sm.CustomerId = t.OldId
                WHERE sm.CustomerId IS NOT NULL
            ");

            // Now drop the Customers table
            migrationBuilder.DropTable(
                name: "Customers");

            // Recreate Customers table with string Id
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    BusinessName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CustomerType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByFullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customers_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Customers_AspNetUsers_Id",
                        column: x => x.Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Restore data from temporary table
            migrationBuilder.Sql(@"
                INSERT INTO Customers (Id, BusinessName, CustomerType, IsActive, CreatedAt, CreatedByFullName, CreatedById)
                SELECT UserId, BusinessName, CustomerType, IsActive, CreatedAt, CreatedByFullName, CreatedById
                FROM TempCustomerData
            ");

            // Drop temporary table
            migrationBuilder.DropTable(
                name: "TempCustomerData");

            // Now make the foreign key columns NOT NULL after data is updated
            migrationBuilder.AlterColumn<string>(
                name: "CustomerId",
                table: "ScheduleMaintenances",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerId",
                table: "Quotations",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerId",
                table: "FridgeRequests",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReportedById",
                table: "Faults",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerId",
                table: "Allocations",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            // Recreate foreign keys
            migrationBuilder.AddForeignKey(
                name: "FK_Allocations_Customers_CustomerId",
                table: "Allocations",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Faults_Customers_ReportedById",
                table: "Faults",
                column: "ReportedById",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FridgeRequests_Customers_CustomerId",
                table: "FridgeRequests",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Fridges_Customers_CustomerId",
                table: "Fridges",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Quotations_Customers_CustomerId",
                table: "Quotations",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
            // This migration is too complex to safely reverse
            // Recommend creating a new migration instead of using Down
            throw new NotSupportedException("This migration cannot be reverted due to data type changes. Create a new migration instead.");
        }
    }
}