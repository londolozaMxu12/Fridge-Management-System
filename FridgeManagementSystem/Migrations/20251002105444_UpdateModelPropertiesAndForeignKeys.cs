using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FridgeManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModelPropertiesAndForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Faults_FaultTechnicians_FaultTechnicianId",
                table: "Faults");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRecords_Employees_MaintenanceTechnicianId",
                table: "MaintenanceRecords");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_MaintenanceRecords_MaintenanceTechs_MaintenanceTechId",
            //    table: "MaintenanceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequests_PurchasingManagers_PurchasingManagerId",
                table: "PurchaseRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchasingOrders_PurchasingManagers_PurchasingManagerId",
                table: "PurchasingOrders");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseRequests_PurchasingManagerId",
                table: "PurchaseRequests");

            //migrationBuilder.DropIndex(
            //    name: "IX_MaintenanceRecords_MaintenanceTechId",
            //    table: "MaintenanceRecords");

            migrationBuilder.DropColumn(
                name: "PurchasingManagerId",
                table: "PurchaseRequests");

            //migrationBuilder.DropColumn(
            //    name: "MaintenanceTechId",
            //    table: "MaintenanceRecords");

            migrationBuilder.RenameColumn(
                name: "PurchasingManagerId",
                table: "PurchasingOrders",
                newName: "EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchasingOrders_PurchasingManagerId",
                table: "PurchasingOrders",
                newName: "IX_PurchasingOrders_EmployeeId");

            migrationBuilder.RenameColumn(
                name: "MaintenanceTechnicianId",
                table: "MaintenanceRecords",
                newName: "EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_MaintenanceRecords_MaintenanceTechnicianId",
                table: "MaintenanceRecords",
                newName: "IX_MaintenanceRecords_EmployeeId");

            migrationBuilder.RenameColumn(
                name: "FaultTechnicianId",
                table: "Faults",
                newName: "EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_Faults_FaultTechnicianId",
                table: "Faults",
                newName: "IX_Faults_EmployeeId");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ShoppingCart",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "PurchasingOrders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "PurchasingOrders",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "EmployeeId",
                table: "PurchaseRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCart_UserId",
                table: "ShoppingCart",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchasingOrders_UserId",
                table: "PurchasingOrders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequests_EmployeeId",
                table: "PurchaseRequests",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Faults_Employees_EmployeeId",
                table: "Faults",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRecords_Employees_EmployeeId",
                table: "MaintenanceRecords",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequests_Employees_EmployeeId",
                table: "PurchaseRequests",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchasingOrders_AspNetUsers_UserId",
                table: "PurchasingOrders",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchasingOrders_Employees_EmployeeId",
                table: "PurchasingOrders",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingCart_AspNetUsers_UserId",
                table: "ShoppingCart",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Faults_Employees_EmployeeId",
                table: "Faults");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRecords_Employees_EmployeeId",
                table: "MaintenanceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequests_Employees_EmployeeId",
                table: "PurchaseRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchasingOrders_AspNetUsers_UserId",
                table: "PurchasingOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchasingOrders_Employees_EmployeeId",
                table: "PurchasingOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCart_AspNetUsers_UserId",
                table: "ShoppingCart");

            migrationBuilder.DropIndex(
                name: "IX_ShoppingCart_UserId",
                table: "ShoppingCart");

            migrationBuilder.DropIndex(
                name: "IX_PurchasingOrders_UserId",
                table: "PurchasingOrders");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseRequests_EmployeeId",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "PurchasingOrders");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PurchasingOrders");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "PurchaseRequests");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "PurchasingOrders",
                newName: "PurchasingManagerId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchasingOrders_EmployeeId",
                table: "PurchasingOrders",
                newName: "IX_PurchasingOrders_PurchasingManagerId");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "MaintenanceRecords",
                newName: "MaintenanceTechnicianId");

            migrationBuilder.RenameIndex(
                name: "IX_MaintenanceRecords_EmployeeId",
                table: "MaintenanceRecords",
                newName: "IX_MaintenanceRecords_MaintenanceTechnicianId");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "Faults",
                newName: "FaultTechnicianId");

            migrationBuilder.RenameIndex(
                name: "IX_Faults_EmployeeId",
                table: "Faults",
                newName: "IX_Faults_FaultTechnicianId");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ShoppingCart",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "PurchasingManagerId",
                table: "PurchaseRequests",
                type: "int",
                nullable: true);

            //migrationBuilder.AddColumn<int>(
            //    name: "MaintenanceTechId",
            //    table: "MaintenanceRecords",
            //    type: "int",
            //    nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequests_PurchasingManagerId",
                table: "PurchaseRequests",
                column: "PurchasingManagerId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_MaintenanceRecords_MaintenanceTechId",
            //    table: "MaintenanceRecords",
            //    column: "MaintenanceTechId");

            migrationBuilder.AddForeignKey(
                name: "FK_Faults_FaultTechnicians_FaultTechnicianId",
                table: "Faults",
                column: "FaultTechnicianId",
                principalTable: "FaultTechnicians",
                principalColumn: "FaultTechnicianId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRecords_Employees_MaintenanceTechnicianId",
                table: "MaintenanceRecords",
                column: "MaintenanceTechnicianId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_MaintenanceRecords_MaintenanceTechs_MaintenanceTechId",
            //    table: "MaintenanceRecords",
            //    column: "MaintenanceTechId",
            //    principalTable: "MaintenanceTechs",
            //    principalColumn: "MaintenanceTechId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequests_PurchasingManagers_PurchasingManagerId",
                table: "PurchaseRequests",
                column: "PurchasingManagerId",
                principalTable: "PurchasingManagers",
                principalColumn: "PurchasingManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchasingOrders_PurchasingManagers_PurchasingManagerId",
                table: "PurchasingOrders",
                column: "PurchasingManagerId",
                principalTable: "PurchasingManagers",
                principalColumn: "PurchasingManagerId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
