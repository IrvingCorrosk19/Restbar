using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestBar.Migrations;

/// <summary>
/// RB-1002 — proven hot-path indexes (idempotent SQL). Does not alter business logic.
/// </summary>
public partial class Rb1002PerformanceIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
-- Audit list: CompanyId + Timestamp DESC (81k+ rows, OrderBy Timestamp)
CREATE INDEX IF NOT EXISTS IX_audit_logs_company_timestamp
    ON public.audit_logs (""CompanyId"", ""timestamp"" DESC);

CREATE INDEX IF NOT EXISTS IX_audit_logs_timestamp
    ON public.audit_logs (""timestamp"" DESC);

CREATE INDEX IF NOT EXISTS IX_audit_logs_company_module_timestamp
    ON public.audit_logs (""CompanyId"", ""Module"", ""timestamp"" DESC);

CREATE INDEX IF NOT EXISTS IX_audit_logs_company_error_timestamp
    ON public.audit_logs (""CompanyId"", ""IsError"", ""timestamp"" DESC)
    WHERE ""IsError"" = true;

-- Kitchen / POS active board: status + opened_at (Seq Scan today)
CREATE INDEX IF NOT EXISTS IX_orders_status_opened
    ON public.orders (status, opened_at);

CREATE INDEX IF NOT EXISTS IX_orders_company_status_opened
    ON public.orders (""CompanyId"", status, opened_at);

-- Closed sales / analytics window (snapshot had it; ensure live)
CREATE INDEX IF NOT EXISTS IX_orders_branch_closed
    ON public.orders (""BranchId"", closed_at)
    WHERE closed_at IS NOT NULL;

CREATE INDEX IF NOT EXISTS IX_orders_company_closed
    ON public.orders (""CompanyId"", closed_at)
    WHERE closed_at IS NOT NULL;

-- Order items kitchen board
CREATE INDEX IF NOT EXISTS IX_order_items_kitchen_status_sent
    ON public.order_items (prepared_by_station_id, kitchen_status, sent_at)
    WHERE kitchen_status IN ('Pending', 'Sent');

-- Customer search by company
CREATE INDEX IF NOT EXISTS IX_customers_company_name
    ON public.customers (""CompanyId"", full_name);

CREATE INDEX IF NOT EXISTS IX_customers_company_email
    ON public.customers (""CompanyId"", email);

-- Product POS browse
CREATE INDEX IF NOT EXISTS IX_products_company_branch_active
    ON public.products (company_id, branch_id, is_active);

ANALYZE public.audit_logs;
ANALYZE public.orders;
ANALYZE public.order_items;
ANALYZE public.customers;
ANALYZE public.products;
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
DROP INDEX IF EXISTS IX_audit_logs_company_timestamp;
DROP INDEX IF EXISTS IX_audit_logs_timestamp;
DROP INDEX IF EXISTS IX_audit_logs_company_module_timestamp;
DROP INDEX IF EXISTS IX_audit_logs_company_error_timestamp;
DROP INDEX IF EXISTS IX_orders_status_opened;
DROP INDEX IF EXISTS IX_orders_company_status_opened;
DROP INDEX IF EXISTS IX_orders_branch_closed;
DROP INDEX IF EXISTS IX_orders_company_closed;
DROP INDEX IF EXISTS IX_order_items_kitchen_status_sent;
DROP INDEX IF EXISTS IX_customers_company_name;
DROP INDEX IF EXISTS IX_customers_company_email;
DROP INDEX IF EXISTS IX_products_company_branch_active;
");
    }
}
