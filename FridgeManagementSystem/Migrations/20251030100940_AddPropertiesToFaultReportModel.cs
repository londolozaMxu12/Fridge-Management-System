using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FridgeManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertiesToFaultReportModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           
            migrationBuilder.DropForeignKey(
                name: "FK_FaultReports_MaintenanceRecords_MaintenanceRecordId",
                table: "FaultReports");

            migrationBuilder.DropIndex(
                name: "IX_FaultReports_MaintenanceRecordId",
                table: "FaultReports");


            migrationBuilder.RenameColumn(
                name: "ReportDate",
                table: "FaultReports",
                newName: "ReporteDate");

            migrationBuilder.AddColumn<int>(
                name: "FaultReortId",
                table: "MaintenanceRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FaultReportId",
                table: "MaintenanceRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "FaultReports",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "FaultReports",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<string>(
                name: "CustomerId",
                table: "FaultReports",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "FridgeId",
                table: "FaultReports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_FaultReportId",
                table: "MaintenanceRecords",
                column: "FaultReportId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FaultReports_CustomerId",
                table: "FaultReports",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_FaultReports_FridgeId",
                table: "FaultReports",
                column: "FridgeId");


            migrationBuilder.AddForeignKey(
                name: "FK_FaultReports_Customers_CustomerId",
                table: "FaultReports",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FaultReports_Fridges_FridgeId",
                table: "FaultReports",
                column: "FridgeId",
                principalTable: "Fridges",
                principalColumn: "FridgeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRecords_FaultReports_FaultReportId",
                table: "MaintenanceRecords",
                column: "FaultReportId",
                principalTable: "FaultReports",
                principalColumn: "FaultReportId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Allocations_Fridges_FridgeId8",
                table: "Allocations");

            migrationBuilder.DropForeignKey(
                name: "FK_FaultReports_Customers_CustomerId",
                table: "FaultReports");

            migrationBuilder.DropForeignKey(
                name: "FK_FaultReports_Fridges_FridgeId",
                table: "FaultReports");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRecords_FaultReports_FaultReportId",
                table: "MaintenanceRecords");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceRecords_FaultReportId",
                table: "MaintenanceRecords");

            migrationBuilder.DropIndex(
                name: "IX_FaultReports_CustomerId",
                table: "FaultReports");

            migrationBuilder.DropIndex(
                name: "IX_FaultReports_FridgeId",
                table: "FaultReports");

            migrationBuilder.DropIndex(
                name: "IX_Allocations_FridgeId8",
                table: "Allocations");

            migrationBuilder.DropColumn(
                name: "FaultReortId",
                table: "MaintenanceRecords");

            migrationBuilder.DropColumn(
                name: "FaultReportId",
                table: "MaintenanceRecords");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "FaultReports");

            migrationBuilder.DropColumn(
                name: "FridgeId",
                table: "FaultReports");

            migrationBuilder.RenameColumn(
                name: "ReporteDate",
                table: "FaultReports",
                newName: "ReportDate");

            migrationBuilder.RenameColumn(
                name: "FridgeId8",
                table: "Allocations",
                newName: "FridgeId6");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "FaultReports",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "FaultReports",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.CreateIndex(
                name: "IX_FaultReports_MaintenanceRecordId",
                table: "FaultReports",
                column: "MaintenanceRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Allocations_FridgeId6",
                table: "Allocations",
                column: "FridgeId6",
                unique: true,
                filter: "[FridgeId6] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Allocations_Fridges_FridgeId6",
                table: "Allocations",
                column: "FridgeId6",
                principalTable: "Fridges",
                principalColumn: "FridgeId");

            migrationBuilder.AddForeignKey(
                name: "FK_FaultReports_MaintenanceRecords_MaintenanceRecordId",
                table: "FaultReports",
                column: "MaintenanceRecordId",
                principalTable: "MaintenanceRecords",
                principalColumn: "MaintenanceRecordId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
