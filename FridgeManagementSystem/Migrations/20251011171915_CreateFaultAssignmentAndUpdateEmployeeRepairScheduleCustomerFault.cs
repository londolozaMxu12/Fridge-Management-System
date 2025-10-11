using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FridgeManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class CreateFaultAssignmentAndUpdateEmployeeRepairScheduleCustomerFault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Faults_Customers_CustomerId",
                table: "Faults");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairSchedules_Employees_FaultTechnicianId",
                table: "RepairSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairSchedules_Faults_FaultId",
                table: "RepairSchedules");

            migrationBuilder.DropIndex(
                name: "IX_RepairSchedules_FaultId",
                table: "RepairSchedules");

            migrationBuilder.DropIndex(
                name: "IX_RepairSchedules_FaultTechnicianId",
                table: "RepairSchedules");

            migrationBuilder.RenameColumn(
                name: "FaultTechnicianId",
                table: "RepairSchedules",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "Faults",
                newName: "ReportedById");

            migrationBuilder.RenameIndex(
                name: "IX_Faults_CustomerId",
                table: "Faults",
                newName: "IX_Faults_ReportedById");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "RepairSchedules",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<int>(
                name: "AssignedTechnicianId",
                table: "RepairSchedules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "RepairSchedules",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "RepairSchedules",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedHours",
                table: "RepairSchedules",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "RepairSchedules",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FaultId",
                table: "MaintenanceRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Faults",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<int>(
                name: "AssignedTechnicianId",
                table: "Faults",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Faults",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Faults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ResolutionNotes",
                table: "Faults",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledDate",
                table: "Faults",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Faults",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Faults",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FaultAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RepairScheduleId = table.Column<int>(type: "int", nullable: false),
                    TechnicianId = table.Column<int>(type: "int", nullable: false),
                    AssignedHours = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaultAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FaultAssignments_Employees_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FaultAssignments_RepairSchedules_RepairScheduleId",
                        column: x => x.RepairScheduleId,
                        principalTable: "RepairSchedules",
                        principalColumn: "RepairScheduleId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RepairSchedules_AssignedTechnicianId",
                table: "RepairSchedules",
                column: "AssignedTechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairSchedules_CreatedById",
                table: "RepairSchedules",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_RepairSchedules_FaultId",
                table: "RepairSchedules",
                column: "FaultId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_FaultId",
                table: "MaintenanceRecords",
                column: "FaultId");

            migrationBuilder.CreateIndex(
                name: "IX_Faults_AssignedTechnicianId",
                table: "Faults",
                column: "AssignedTechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_FaultAssignments_RepairScheduleId",
                table: "FaultAssignments",
                column: "RepairScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_FaultAssignments_TechnicianId",
                table: "FaultAssignments",
                column: "TechnicianId");

            migrationBuilder.AddForeignKey(
                name: "FK_Faults_Customers_ReportedById",
                table: "Faults",
                column: "ReportedById",
                principalTable: "Customers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Faults_Employees_AssignedTechnicianId",
                table: "Faults",
                column: "AssignedTechnicianId",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRecords_Faults_FaultId",
                table: "MaintenanceRecords",
                column: "FaultId",
                principalTable: "Faults",
                principalColumn: "FaultId");

            migrationBuilder.AddForeignKey(
                name: "FK_RepairSchedules_AspNetUsers_CreatedById",
                table: "RepairSchedules",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RepairSchedules_Employees_AssignedTechnicianId",
                table: "RepairSchedules",
                column: "AssignedTechnicianId",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RepairSchedules_Faults_FaultId",
                table: "RepairSchedules",
                column: "FaultId",
                principalTable: "Faults",
                principalColumn: "FaultId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Faults_Customers_ReportedById",
                table: "Faults");

            migrationBuilder.DropForeignKey(
                name: "FK_Faults_Employees_AssignedTechnicianId",
                table: "Faults");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRecords_Faults_FaultId",
                table: "MaintenanceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairSchedules_AspNetUsers_CreatedById",
                table: "RepairSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairSchedules_Employees_AssignedTechnicianId",
                table: "RepairSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairSchedules_Faults_FaultId",
                table: "RepairSchedules");

            migrationBuilder.DropTable(
                name: "FaultAssignments");

            migrationBuilder.DropIndex(
                name: "IX_RepairSchedules_AssignedTechnicianId",
                table: "RepairSchedules");

            migrationBuilder.DropIndex(
                name: "IX_RepairSchedules_CreatedById",
                table: "RepairSchedules");

            migrationBuilder.DropIndex(
                name: "IX_RepairSchedules_FaultId",
                table: "RepairSchedules");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceRecords_FaultId",
                table: "MaintenanceRecords");

            migrationBuilder.DropIndex(
                name: "IX_Faults_AssignedTechnicianId",
                table: "Faults");

            migrationBuilder.DropColumn(
                name: "AssignedTechnicianId",
                table: "RepairSchedules");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "RepairSchedules");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "RepairSchedules");

            migrationBuilder.DropColumn(
                name: "EstimatedHours",
                table: "RepairSchedules");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "RepairSchedules");

            migrationBuilder.DropColumn(
                name: "FaultId",
                table: "MaintenanceRecords");

            migrationBuilder.DropColumn(
                name: "AssignedTechnicianId",
                table: "Faults");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Faults");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Faults");

            migrationBuilder.DropColumn(
                name: "ResolutionNotes",
                table: "Faults");

            migrationBuilder.DropColumn(
                name: "ScheduledDate",
                table: "Faults");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Faults");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Faults");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "RepairSchedules",
                newName: "FaultTechnicianId");

            migrationBuilder.RenameColumn(
                name: "ReportedById",
                table: "Faults",
                newName: "CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_Faults_ReportedById",
                table: "Faults",
                newName: "IX_Faults_CustomerId");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "RepairSchedules",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Faults",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.CreateIndex(
                name: "IX_RepairSchedules_FaultId",
                table: "RepairSchedules",
                column: "FaultId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RepairSchedules_FaultTechnicianId",
                table: "RepairSchedules",
                column: "FaultTechnicianId");

            migrationBuilder.AddForeignKey(
                name: "FK_Faults_Customers_CustomerId",
                table: "Faults",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairSchedules_Employees_FaultTechnicianId",
                table: "RepairSchedules",
                column: "FaultTechnicianId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairSchedules_Faults_FaultId",
                table: "RepairSchedules",
                column: "FaultId",
                principalTable: "Faults",
                principalColumn: "FaultId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
