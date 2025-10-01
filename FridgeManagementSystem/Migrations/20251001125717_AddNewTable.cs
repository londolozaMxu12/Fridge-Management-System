using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FridgeManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddNewTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Fridges_FridgeId",
                table: "OrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_PurchasingOrders_PurchasingOrderId",
                table: "OrderDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderDetails",
                table: "OrderDetails");

            migrationBuilder.RenameTable(
                name: "OrderDetails",
                newName: "PurchasingOrderDetails");

            migrationBuilder.RenameIndex(
                name: "IX_OrderDetails_PurchasingOrderId",
                table: "PurchasingOrderDetails",
                newName: "IX_PurchasingOrderDetails_PurchasingOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderDetails_FridgeId",
                table: "PurchasingOrderDetails",
                newName: "IX_PurchasingOrderDetails_FridgeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PurchasingOrderDetails",
                table: "PurchasingOrderDetails",
                column: "PurchasingOrderDetailsId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchasingOrderDetails_Fridges_FridgeId",
                table: "PurchasingOrderDetails",
                column: "FridgeId",
                principalTable: "Fridges",
                principalColumn: "FridgeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchasingOrderDetails_PurchasingOrders_PurchasingOrderId",
                table: "PurchasingOrderDetails",
                column: "PurchasingOrderId",
                principalTable: "PurchasingOrders",
                principalColumn: "PurchasingOrderId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchasingOrderDetails_Fridges_FridgeId",
                table: "PurchasingOrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchasingOrderDetails_PurchasingOrders_PurchasingOrderId",
                table: "PurchasingOrderDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PurchasingOrderDetails",
                table: "PurchasingOrderDetails");

            migrationBuilder.RenameTable(
                name: "PurchasingOrderDetails",
                newName: "OrderDetails");

            migrationBuilder.RenameIndex(
                name: "IX_PurchasingOrderDetails_PurchasingOrderId",
                table: "OrderDetails",
                newName: "IX_OrderDetails_PurchasingOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchasingOrderDetails_FridgeId",
                table: "OrderDetails",
                newName: "IX_OrderDetails_FridgeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderDetails",
                table: "OrderDetails",
                column: "PurchasingOrderDetailsId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Fridges_FridgeId",
                table: "OrderDetails",
                column: "FridgeId",
                principalTable: "Fridges",
                principalColumn: "FridgeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_PurchasingOrders_PurchasingOrderId",
                table: "OrderDetails",
                column: "PurchasingOrderId",
                principalTable: "PurchasingOrders",
                principalColumn: "PurchasingOrderId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
