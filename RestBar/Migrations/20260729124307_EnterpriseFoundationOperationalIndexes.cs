using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestBar.Migrations
{
    /// <inheritdoc />
    public partial class EnterpriseFoundationOperationalIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Additive only — do not drop existing FK indexes (may be missing on some DBs).
            migrationBuilder.Sql(@"
CREATE INDEX IF NOT EXISTS ""IX_shifts_user_active"" ON shifts (user_id, is_active);
CREATE INDEX IF NOT EXISTS ""IX_payments_branch_paid_at"" ON payments (""BranchId"", paid_at);
CREATE INDEX IF NOT EXISTS ""IX_orders_branch_status_opened"" ON orders (""BranchId"", status, opened_at);
CREATE INDEX IF NOT EXISTS ""IX_orders_table_status"" ON orders (table_id, status);
CREATE INDEX IF NOT EXISTS ""IX_order_items_order_status"" ON order_items (order_id, status);
CREATE INDEX IF NOT EXISTS ""IX_order_items_station_status"" ON order_items (prepared_by_station_id, status);
CREATE INDEX IF NOT EXISTS ""IX_inv_mov_company_created"" ON inventory_movements (""CompanyId"", created_at);
CREATE INDEX IF NOT EXISTS ""IX_inv_mov_product_created"" ON inventory_movements (product_id, created_at);
CREATE INDEX IF NOT EXISTS ""IX_discount_policies_company_active"" ON ""DiscountPolicies"" (""CompanyId"", ""IsActive"");
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP INDEX IF EXISTS ""IX_shifts_user_active"";
DROP INDEX IF EXISTS ""IX_payments_branch_paid_at"";
DROP INDEX IF EXISTS ""IX_orders_branch_status_opened"";
DROP INDEX IF EXISTS ""IX_orders_table_status"";
DROP INDEX IF EXISTS ""IX_order_items_order_status"";
DROP INDEX IF EXISTS ""IX_order_items_station_status"";
DROP INDEX IF EXISTS ""IX_inv_mov_company_created"";
DROP INDEX IF EXISTS ""IX_inv_mov_product_created"";
DROP INDEX IF EXISTS ""IX_discount_policies_company_active"";
");
        }
    }
}
