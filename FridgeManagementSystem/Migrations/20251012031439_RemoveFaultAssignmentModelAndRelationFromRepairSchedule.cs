using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FridgeManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFaultAssignmentModelAndRelationFromRepairSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FaultAssignments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FaultAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RepairScheduleId = table.Column<int>(type: "int", nullable: false),
                    TechnicianId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedHours = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false)
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
                name: "IX_FaultAssignments_RepairScheduleId",
                table: "FaultAssignments",
                column: "RepairScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_FaultAssignments_TechnicianId",
                table: "FaultAssignments",
                column: "TechnicianId");
        }
    }
}
