using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FridgeManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddModelsAndUpdateProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_AspNetUsers_CreatedById",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_AspNetUsers_UserId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Fridges_Customers_CustomerId",
                table: "Fridges");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRecords_Fridges_FridgeId",
                table: "MaintenanceRecords");

            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Fridges");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "Fridges");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Fridges");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Allocations");

            //migrationBuilder.RenameColumn(
            //    name: "date",
            //    table: "MaintenanceRecords",
            //    newName: "ServiceDate");

            //migrationBuilder.RenameColumn(
            //    name: "ScrapDate",
            //    table: "Fridges",
            //    newName: "ServiceDate");

            //migrationBuilder.RenameColumn(
            //    name: "CreatedAt",
            //    table: "Fridges",
            //    newName: "AcquisitionDate");

            //migrationBuilder.RenameColumn(
            //    name: "StartDate",
            //    table: "Allocations",
            //    newName: "AllocationDate");

            //migrationBuilder.RenameColumn(
            //    name: "EndDate",
            //    table: "Allocations",
            //    newName: "ServiceDate");

            migrationBuilder.AddColumn<int>(
                name: "OrderStatusId",
                table: "PurchasingOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "OrderDetails",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            //migrationBuilder.AlterColumn<int>(
            //    name: "MaintenanceTechId",
            //    table: "MaintenanceRecords",
            //    type: "int",
            //    nullable: true,
            //    oldClrType: typeof(int),
            //    oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "MaintenanceRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "MaintenanceTechnicianId",
                table: "MaintenanceRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextServiceDate",
                table: "MaintenanceRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Fridges",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "SerialNumber",
                table: "Fridges",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            //migrationBuilder.AlterColumn<DateTime>(
            //    name: "PurchaseDate",
            //    table: "Fridges",
            //    type: "datetime2",
            //    nullable: false,
            //    defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
            //    oldClrType: typeof(DateTime),
            //    oldType: "datetime2",
            //    oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "Fridges",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "AllocationDate",
                table: "Fridges",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "Fridges",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.Sql(@"
    UPDATE Fridges 
    SET CreatedById = (SELECT TOP 1 Id FROM AspNetUsers)
    WHERE CreatedById IS NULL;
");
            migrationBuilder.AlterColumn<string>(
    name: "CreatedById",
    table: "Fridges",
    type: "nvarchar(450)",
    nullable: false);

            migrationBuilder.AddColumn<int>(
                name: "FridgeTypeId",
                table: "Fridges",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Fridges",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextServiceDate",
                table: "Fridges",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AllocatedById",
                table: "Allocations",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.Sql(@"
    UPDATE Allocations 
    SET AllocatedById = (SELECT TOP 1 Id FROM AspNetUsers)
    WHERE AllocatedById IS NULL;
");

            migrationBuilder.AlterColumn<string>(
    name: "AllocatedById",
    table: "Allocations",
    type: "nvarchar(450)",
    nullable: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Allocations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "FridgeType",
                columns: table => new
                {
                    FridgeTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FridgeType", x => x.FridgeTypeId);
                });

            migrationBuilder.CreateTable(
                name: "OrderStatus",
                columns: table => new
                {
                    OrderStatusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderStatusName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatus", x => x.OrderStatusId);
                });

            migrationBuilder.CreateTable(
                name: "ShoppingCart",
                columns: table => new
                {
                    ShoppingCartId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingCart", x => x.ShoppingCartId);
                });

            migrationBuilder.CreateTable(
                name: "CartDetails",
                columns: table => new
                {
                    CartDetailsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShoppingCartId = table.Column<int>(type: "int", nullable: false),
                    FridgeId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartDetails", x => x.CartDetailsId);
                    table.ForeignKey(
                        name: "FK_CartDetails_Fridges_FridgeId",
                        column: x => x.FridgeId,
                        principalTable: "Fridges",
                        principalColumn: "FridgeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartDetails_ShoppingCart_ShoppingCartId",
                        column: x => x.ShoppingCartId,
                        principalTable: "ShoppingCart",
                        principalColumn: "ShoppingCartId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchasingOrders_OrderStatusId",
                table: "PurchasingOrders",
                column: "OrderStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_MaintenanceTechnicianId",
                table: "MaintenanceRecords",
                column: "MaintenanceTechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_Fridges_CreatedById",
                table: "Fridges",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Fridges_FridgeTypeId",
                table: "Fridges",
                column: "FridgeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Allocations_AllocatedById",
                table: "Allocations",
                column: "AllocatedById");

            migrationBuilder.CreateIndex(
                name: "IX_CartDetails_FridgeId",
                table: "CartDetails",
                column: "FridgeId");

            migrationBuilder.CreateIndex(
                name: "IX_CartDetails_ShoppingCartId",
                table: "CartDetails",
                column: "ShoppingCartId");

            migrationBuilder.AddForeignKey(
                name: "FK_Allocations_AspNetUsers_AllocatedById",
                table: "Allocations",
                column: "AllocatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_AspNetUsers_CreatedById",
                table: "Customers",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_AspNetUsers_UserId",
                table: "Customers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Fridges_AspNetUsers_CreatedById",
                table: "Fridges",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Fridges_Customers_CustomerId",
                table: "Fridges",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Fridges_FridgeType_FridgeTypeId",
                table: "Fridges",
                column: "FridgeTypeId",
                principalTable: "FridgeType",
                principalColumn: "FridgeTypeId");

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
                name: "FK_PurchasingOrders_OrderStatus_OrderStatusId",
                table: "PurchasingOrders",
                column: "OrderStatusId",
                principalTable: "OrderStatus",
                principalColumn: "OrderStatusId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Allocations_AspNetUsers_AllocatedById",
                table: "Allocations");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_AspNetUsers_CreatedById",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_AspNetUsers_UserId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Fridges_AspNetUsers_CreatedById",
                table: "Fridges");

            migrationBuilder.DropForeignKey(
                name: "FK_Fridges_Customers_CustomerId",
                table: "Fridges");

            migrationBuilder.DropForeignKey(
                name: "FK_Fridges_FridgeType_FridgeTypeId",
                table: "Fridges");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRecords_Employees_MaintenanceTechnicianId",
                table: "MaintenanceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRecords_Fridges_FridgeId",
                table: "MaintenanceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchasingOrders_OrderStatus_OrderStatusId",
                table: "PurchasingOrders");

            migrationBuilder.DropTable(
                name: "CartDetails");

            migrationBuilder.DropTable(
                name: "FridgeType");

            migrationBuilder.DropTable(
                name: "OrderStatus");

            migrationBuilder.DropTable(
                name: "ShoppingCart");

            migrationBuilder.DropIndex(
                name: "IX_PurchasingOrders_OrderStatusId",
                table: "PurchasingOrders");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceRecords_MaintenanceTechnicianId",
                table: "MaintenanceRecords");

            migrationBuilder.DropIndex(
                name: "IX_Fridges_CreatedById",
                table: "Fridges");

            migrationBuilder.DropIndex(
                name: "IX_Fridges_FridgeTypeId",
                table: "Fridges");

            migrationBuilder.DropIndex(
                name: "IX_Allocations_AllocatedById",
                table: "Allocations");

            migrationBuilder.DropColumn(
                name: "OrderStatusId",
                table: "PurchasingOrders");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "MaintenanceRecords");

            migrationBuilder.DropColumn(
                name: "MaintenanceTechnicianId",
                table: "MaintenanceRecords");

            migrationBuilder.DropColumn(
                name: "NextServiceDate",
                table: "MaintenanceRecords");

            migrationBuilder.DropColumn(
                name: "AllocationDate",
                table: "Fridges");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Fridges");

            migrationBuilder.DropColumn(
                name: "FridgeTypeId",
                table: "Fridges");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Fridges");

            migrationBuilder.DropColumn(
                name: "NextServiceDate",
                table: "Fridges");

            migrationBuilder.DropColumn(
                name: "AllocatedById",
                table: "Allocations");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Allocations");

            //migrationBuilder.RenameColumn(
            //    name: "ServiceDate",
            //    table: "MaintenanceRecords",
            //    newName: "date");

            //migrationBuilder.RenameColumn(
            //    name: "ServiceDate",
            //    table: "Fridges",
            //    newName: "ScrapDate");

            //migrationBuilder.RenameColumn(
            //    name: "AcquisitionDate",
            //    table: "Fridges",
            //    newName: "CreatedAt");

            //migrationBuilder.RenameColumn(
            //    name: "ServiceDate",
            //    table: "Allocations",
            //    newName: "EndDate");

            //migrationBuilder.RenameColumn(
            //    name: "AllocationDate",
            //    table: "Allocations",
            //    newName: "StartDate");

            //migrationBuilder.AlterColumn<int>(
            //    name: "MaintenanceTechId",
            //    table: "MaintenanceRecords",
            //    type: "int",
            //    nullable: false,
            //    defaultValue: 0,
            //    oldClrType: typeof(int),
            //    oldType: "int",
            //    oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Fridges",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "SerialNumber",
                table: "Fridges",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            //migrationBuilder.AlterColumn<DateTime>(
            //    name: "PurchaseDate",
            //    table: "Fridges",
            //    type: "datetime2",
            //    nullable: true,
            //    oldClrType: typeof(DateTime),
            //    oldType: "datetime2");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "Fridges",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "Fridges",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Fridges",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Fridges",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Allocations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_AspNetUsers_CreatedById",
                table: "Customers",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_AspNetUsers_UserId",
                table: "Customers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Fridges_Customers_CustomerId",
                table: "Fridges",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRecords_MaintenanceTechs_MaintenanceTechId",
                table: "MaintenanceRecords",
                column: "MaintenanceTechId",
                principalTable: "MaintenanceTechs",
                principalColumn: "MaintenanceTechId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
