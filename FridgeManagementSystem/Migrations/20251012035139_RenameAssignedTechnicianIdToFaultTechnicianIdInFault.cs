using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FridgeManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class RenameAssignedTechnicianIdToFaultTechnicianIdInFault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Faults_Employees_AssignedTechnicianId",
                table: "Faults");

            migrationBuilder.RenameColumn(
                name: "AssignedTechnicianId",
                table: "Faults",
                newName: "FaultTechnicianId");

            migrationBuilder.RenameIndex(
                name: "IX_Faults_AssignedTechnicianId",
                table: "Faults",
                newName: "IX_Faults_FaultTechnicianId");

            migrationBuilder.AddForeignKey(
                name: "FK_Faults_Employees_FaultTechnicianId",
                table: "Faults",
                column: "FaultTechnicianId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Faults_Employees_FaultTechnicianId",
                table: "Faults");

            migrationBuilder.RenameColumn(
                name: "FaultTechnicianId",
                table: "Faults",
                newName: "AssignedTechnicianId");

            migrationBuilder.RenameIndex(
                name: "IX_Faults_FaultTechnicianId",
                table: "Faults",
                newName: "IX_Faults_AssignedTechnicianId");

            migrationBuilder.AddForeignKey(
                name: "FK_Faults_Employees_AssignedTechnicianId",
                table: "Faults",
                column: "AssignedTechnicianId",
                principalTable: "Employees",
                principalColumn: "Id");
        }
    }
}
