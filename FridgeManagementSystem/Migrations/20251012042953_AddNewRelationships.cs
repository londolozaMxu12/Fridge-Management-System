using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FridgeManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddNewRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Faults_Employees_EmployeeId",
                table: "Faults");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairSchedules_Employees_AssignedTechnicianId",
                table: "RepairSchedules");

            migrationBuilder.DropIndex(
                name: "IX_Faults_EmployeeId",
                table: "Faults");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "Faults");

            migrationBuilder.RenameColumn(
                name: "AssignedTechnicianId",
                table: "RepairSchedules",
                newName: "FaultTechnicianId");

            migrationBuilder.RenameIndex(
                name: "IX_RepairSchedules_AssignedTechnicianId",
                table: "RepairSchedules",
                newName: "IX_RepairSchedules_FaultTechnicianId");

            migrationBuilder.AddForeignKey(
                name: "FK_RepairSchedules_Employees_FaultTechnicianId",
                table: "RepairSchedules",
                column: "FaultTechnicianId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RepairSchedules_Employees_FaultTechnicianId",
                table: "RepairSchedules");

            migrationBuilder.RenameColumn(
                name: "FaultTechnicianId",
                table: "RepairSchedules",
                newName: "AssignedTechnicianId");

            migrationBuilder.RenameIndex(
                name: "IX_RepairSchedules_FaultTechnicianId",
                table: "RepairSchedules",
                newName: "IX_RepairSchedules_AssignedTechnicianId");

            migrationBuilder.AddColumn<int>(
                name: "EmployeeId",
                table: "Faults",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Faults_EmployeeId",
                table: "Faults",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Faults_Employees_EmployeeId",
                table: "Faults",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RepairSchedules_Employees_AssignedTechnicianId",
                table: "RepairSchedules",
                column: "AssignedTechnicianId",
                principalTable: "Employees",
                principalColumn: "Id");
        }
    }
}
