using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FridgeManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleMaintenance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.AddColumn<int>(
                name: "FaultTechnicianId",
                table: "RepairSchedules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "scheduleMaintenanceId",
                table: "MaintenanceRecords",
                type: "int",
                nullable: true);

            
            migrationBuilder.CreateTable(
                name: "ScheduleMaintenances",
                columns: table => new
                {
                    scheduleMaintenanceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaintenanceTechnicianId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleMaintenances", x => x.scheduleMaintenanceId);
                    table.ForeignKey(
                        name: "FK_ScheduleMaintenances_Employees_MaintenanceTechnicianId",
                        column: x => x.MaintenanceTechnicianId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RepairSchedules_FaultTechnicianId",
                table: "RepairSchedules",
                column: "FaultTechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_scheduleMaintenanceId",
                table: "MaintenanceRecords",
                column: "scheduleMaintenanceId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleMaintenances_MaintenanceTechnicianId",
                table: "ScheduleMaintenances",
                column: "MaintenanceTechnicianId");

           
            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRecords_ScheduleMaintenances_scheduleMaintenanceId",
                table: "MaintenanceRecords",
                column: "scheduleMaintenanceId",
                principalTable: "ScheduleMaintenances",
                principalColumn: "scheduleMaintenanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_RepairSchedules_Employees_FaultTechnicianId",
                table: "RepairSchedules",
                column: "FaultTechnicianId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRecords_ScheduleMaintenances_scheduleMaintenanceId",
                table: "MaintenanceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairSchedules_Employees_FaultTechnicianId",
                table: "RepairSchedules");

            migrationBuilder.DropTable(
                name: "ScheduleMaintenances");

            migrationBuilder.DropIndex(
                name: "IX_RepairSchedules_FaultTechnicianId",
                table: "RepairSchedules");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceRecords_scheduleMaintenanceId",
                table: "MaintenanceRecords");

            migrationBuilder.DropColumn(
                name: "FaultTechnicianId",
                table: "RepairSchedules");

            migrationBuilder.DropColumn(
                name: "scheduleMaintenanceId",
                table: "MaintenanceRecords");

        }
    }
}
