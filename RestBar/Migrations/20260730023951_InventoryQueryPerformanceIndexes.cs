using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestBar.Migrations
{
    /// <inheritdoc />
    public partial class InventoryQueryPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_products_company_id",
                table: "products");

            migrationBuilder.DropIndex(
                name: "IX_product_stock_assignments_branch_id",
                table: "product_stock_assignments");

            migrationBuilder.DropIndex(
                name: "IX_order_items_product_id",
                table: "order_items");

            migrationBuilder.CreateIndex(
                name: "IX_products_company_active_track",
                table: "products",
                columns: new[] { "company_id", "is_active", "track_inventory" });

            migrationBuilder.CreateIndex(
                name: "IX_psa_branch_active",
                table: "product_stock_assignments",
                columns: new[] { "branch_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_orders_branch_created",
                table: "orders",
                columns: new[] { "BranchId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_order_items_product_order",
                table: "order_items",
                columns: new[] { "product_id", "order_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_products_company_active_track",
                table: "products");

            migrationBuilder.DropIndex(
                name: "IX_psa_branch_active",
                table: "product_stock_assignments");

            migrationBuilder.DropIndex(
                name: "IX_orders_branch_created",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "IX_order_items_product_order",
                table: "order_items");

            migrationBuilder.CreateIndex(
                name: "IX_products_company_id",
                table: "products",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_stock_assignments_branch_id",
                table: "product_stock_assignments",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_product_id",
                table: "order_items",
                column: "product_id");
        }
    }
}
