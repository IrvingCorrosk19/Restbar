using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestBar.Migrations;

/// <inheritdoc />
public partial class NativeBiAnalyticsLayer : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
CREATE INDEX IF NOT EXISTS ""IX_orders_branch_closed""
ON orders (""BranchId"", closed_at)
WHERE closed_at IS NOT NULL;
");

        migrationBuilder.Sql(@"
CREATE INDEX IF NOT EXISTS ""IX_inv_mov_branch_created""
ON inventory_movements (""BranchId"", created_at);
");

        migrationBuilder.Sql(@"-- RB-025 Native BI — PostgreSQL analytical functions
-- All functions require p_company_id + p_branch_id (multitenant hard filter).
-- Naming: sp_* mirrors enterprise BI catalog; PostgreSQL implements as FUNCTIONS.

CREATE OR REPLACE FUNCTION sp_sales_summary(
    p_company_id uuid,
    p_branch_id uuid,
    p_start timestamptz,
    p_end timestamptz
)
RETURNS TABLE (
    order_count bigint,
    revenue numeric,
    avg_ticket numeric,
    cancelled_count bigint,
    discount_total numeric,
    completed_count bigint
)
LANGUAGE sql
STABLE
AS $$
    SELECT
        COUNT(*)::bigint AS order_count,
        COALESCE(SUM(o.total_amount) FILTER (WHERE o.status = 'Completed'), 0) AS revenue,
        CASE
            WHEN COUNT(*) FILTER (WHERE o.status = 'Completed') > 0
            THEN COALESCE(SUM(o.total_amount) FILTER (WHERE o.status = 'Completed'), 0)
                 / COUNT(*) FILTER (WHERE o.status = 'Completed')
            ELSE 0
        END AS avg_ticket,
        COUNT(*) FILTER (WHERE o.status = 'Cancelled')::bigint AS cancelled_count,
        COALESCE(SUM(o.discount_amount), 0) AS discount_total,
        COUNT(*) FILTER (WHERE o.status = 'Completed')::bigint AS completed_count
    FROM orders o
    WHERE o.""CompanyId"" = p_company_id
      AND o.""BranchId"" = p_branch_id
      AND o.closed_at IS NOT NULL
      AND o.closed_at >= p_start
      AND o.closed_at < p_end;
$$;

CREATE OR REPLACE FUNCTION sp_hourly_sales(
    p_company_id uuid,
    p_branch_id uuid,
    p_start timestamptz,
    p_end timestamptz
)
RETURNS TABLE (
    sale_hour int,
    order_count bigint,
    revenue numeric
)
LANGUAGE sql
STABLE
AS $$
    SELECT
        EXTRACT(HOUR FROM o.closed_at AT TIME ZONE 'UTC')::int AS sale_hour,
        COUNT(*)::bigint AS order_count,
        COALESCE(SUM(o.total_amount), 0) AS revenue
    FROM orders o
    WHERE o.""CompanyId"" = p_company_id
      AND o.""BranchId"" = p_branch_id
      AND o.status = 'Completed'
      AND o.closed_at IS NOT NULL
      AND o.closed_at >= p_start
      AND o.closed_at < p_end
    GROUP BY 1
    ORDER BY 1;
$$;

CREATE OR REPLACE FUNCTION sp_top_products(
    p_company_id uuid,
    p_branch_id uuid,
    p_start timestamptz,
    p_end timestamptz,
    p_limit int DEFAULT 20
)
RETURNS TABLE (
    product_id uuid,
    product_name text,
    qty_sold numeric,
    revenue numeric,
    cogs_estimate numeric,
    margin_estimate numeric
)
LANGUAGE sql
STABLE
AS $$
    SELECT
        p.id AS product_id,
        p.name::text AS product_name,
        COALESCE(SUM(oi.quantity), 0) AS qty_sold,
        COALESCE(SUM(oi.quantity * oi.unit_price - COALESCE(oi.discount, 0)), 0) AS revenue,
        COALESCE(SUM(
            oi.quantity * COALESCE(oi.theoretical_unit_cost, p.average_cost, p.cost, 0)
        ), 0) AS cogs_estimate,
        COALESCE(SUM(oi.quantity * oi.unit_price - COALESCE(oi.discount, 0)), 0)
          - COALESCE(SUM(oi.quantity * COALESCE(oi.theoretical_unit_cost, p.average_cost, p.cost, 0)), 0)
          AS margin_estimate
    FROM order_items oi
    INNER JOIN orders o ON o.id = oi.order_id
    INNER JOIN products p ON p.id = oi.product_id
    WHERE o.""CompanyId"" = p_company_id
      AND o.""BranchId"" = p_branch_id
      AND o.status = 'Completed'
      AND o.closed_at IS NOT NULL
      AND o.closed_at >= p_start
      AND o.closed_at < p_end
    GROUP BY p.id, p.name
    ORDER BY revenue DESC
    LIMIT GREATEST(COALESCE(p_limit, 20), 1);
$$;

CREATE OR REPLACE FUNCTION sp_waiter_performance(
    p_company_id uuid,
    p_branch_id uuid,
    p_start timestamptz,
    p_end timestamptz
)
RETURNS TABLE (
    user_id uuid,
    waiter_name text,
    order_count bigint,
    revenue numeric,
    avg_ticket numeric
)
LANGUAGE sql
STABLE
AS $$
    SELECT
        u.id AS user_id,
        COALESCE(u.full_name, u.email)::text AS waiter_name,
        COUNT(*)::bigint AS order_count,
        COALESCE(SUM(o.total_amount), 0) AS revenue,
        CASE WHEN COUNT(*) > 0 THEN COALESCE(SUM(o.total_amount), 0) / COUNT(*) ELSE 0 END AS avg_ticket
    FROM orders o
    INNER JOIN users u ON u.id = o.user_id
    WHERE o.""CompanyId"" = p_company_id
      AND o.""BranchId"" = p_branch_id
      AND o.status = 'Completed'
      AND o.closed_at IS NOT NULL
      AND o.closed_at >= p_start
      AND o.closed_at < p_end
      AND o.user_id IS NOT NULL
    GROUP BY u.id, u.full_name, u.email
    ORDER BY revenue DESC;
$$;

CREATE OR REPLACE FUNCTION sp_station_performance(
    p_company_id uuid,
    p_branch_id uuid,
    p_start timestamptz,
    p_end timestamptz
)
RETURNS TABLE (
    station_id uuid,
    station_name text,
    items_processed bigint,
    orders_processed bigint,
    avg_prep_minutes numeric,
    revenue numeric
)
LANGUAGE sql
STABLE
AS $$
    SELECT
        s.id AS station_id,
        s.name::text AS station_name,
        COUNT(oi.id)::bigint AS items_processed,
        COUNT(DISTINCT oi.order_id)::bigint AS orders_processed,
        COALESCE(AVG(
            CASE
                WHEN oi.sent_at IS NOT NULL AND oi.prepared_at IS NOT NULL
                     AND oi.prepared_at >= oi.sent_at
                THEN EXTRACT(EPOCH FROM (oi.prepared_at - oi.sent_at)) / 60.0
                ELSE NULL
            END
        ), 0) AS avg_prep_minutes,
        COALESCE(SUM(oi.quantity * oi.unit_price), 0) AS revenue
    FROM stations s
    LEFT JOIN order_items oi
        ON oi.prepared_by_station_id = s.id
    LEFT JOIN orders o
        ON o.id = oi.order_id
       AND o.""CompanyId"" = p_company_id
       AND o.""BranchId"" = p_branch_id
       AND o.status = 'Completed'
       AND o.closed_at IS NOT NULL
       AND o.closed_at >= p_start
       AND o.closed_at < p_end
    WHERE s.company_id = p_company_id
      AND (s.branch_id IS NULL OR s.branch_id = p_branch_id)
    GROUP BY s.id, s.name
    ORDER BY items_processed DESC;
$$;

CREATE OR REPLACE FUNCTION sp_cash_summary(
    p_company_id uuid,
    p_branch_id uuid,
    p_start timestamptz,
    p_end timestamptz
)
RETURNS TABLE (
    sessions_opened bigint,
    sessions_closed bigint,
    total_sales numeric,
    total_refunds numeric,
    total_paid_in numeric,
    total_paid_out numeric,
    total_variance numeric,
    abs_variance numeric
)
LANGUAGE sql
STABLE
AS $$
    SELECT
        COUNT(*)::bigint AS sessions_opened,
        COUNT(*) FILTER (WHERE cs.status = 'Closed')::bigint AS sessions_closed,
        COALESCE(SUM(cs.total_sales), 0) AS total_sales,
        COALESCE(SUM(cs.total_refunds), 0) AS total_refunds,
        COALESCE(SUM(cs.total_paid_in), 0) AS total_paid_in,
        COALESCE(SUM(cs.total_paid_out), 0) AS total_paid_out,
        COALESCE(SUM(cs.variance), 0) AS total_variance,
        COALESCE(SUM(ABS(COALESCE(cs.variance, 0))), 0) AS abs_variance
    FROM cash_sessions cs
    WHERE cs.company_id = p_company_id
      AND cs.branch_id = p_branch_id
      AND cs.opened_at >= p_start
      AND cs.opened_at < p_end;
$$;

CREATE OR REPLACE FUNCTION sp_inventory_health(
    p_company_id uuid,
    p_branch_id uuid
)
RETURNS TABLE (
    tracked_products bigint,
    low_stock_count bigint,
    zero_stock_count bigint,
    stock_value_estimate numeric,
    waste_qty_30d numeric,
    waste_cost_30d numeric,
    sale_movements_30d bigint
)
LANGUAGE sql
STABLE
AS $$
    WITH stock AS (
        SELECT
            COUNT(*) FILTER (WHERE p.track_inventory)::bigint AS tracked_products,
            COUNT(*) FILTER (
                WHERE p.track_inventory
                  AND p.min_stock IS NOT NULL
                  AND COALESCE(p.stock, 0) <= p.min_stock
            )::bigint AS low_stock_count,
            COUNT(*) FILTER (
                WHERE p.track_inventory AND COALESCE(p.stock, 0) <= 0
            )::bigint AS zero_stock_count,
            COALESCE(SUM(
                CASE WHEN p.track_inventory
                     THEN COALESCE(p.stock, 0) * COALESCE(p.average_cost, p.cost, 0)
                     ELSE 0 END
            ), 0) AS stock_value_estimate
        FROM products p
        WHERE p.company_id = p_company_id
          AND (p.branch_id IS NULL OR p.branch_id = p_branch_id)
          AND p.is_active = true
    ),
    mov AS (
        SELECT
            COALESCE(SUM(im.quantity) FILTER (WHERE im.movement_type = 'Waste'), 0) AS waste_qty_30d,
            COALESCE(SUM(im.quantity * COALESCE(im.unit_cost, 0)) FILTER (WHERE im.movement_type = 'Waste'), 0) AS waste_cost_30d,
            COUNT(*) FILTER (WHERE im.movement_type = 'Sale')::bigint AS sale_movements_30d
        FROM inventory_movements im
        WHERE im.""CompanyId"" = p_company_id
          AND (im.""BranchId"" IS NULL OR im.""BranchId"" = p_branch_id)
          AND im.created_at >= (NOW() AT TIME ZONE 'utc') - INTERVAL '30 days'
    )
    SELECT
        s.tracked_products,
        s.low_stock_count,
        s.zero_stock_count,
        s.stock_value_estimate,
        m.waste_qty_30d,
        m.waste_cost_30d,
        m.sale_movements_30d
    FROM stock s CROSS JOIN mov m;
$$;

CREATE OR REPLACE FUNCTION sp_food_cost_summary(
    p_company_id uuid,
    p_branch_id uuid,
    p_start timestamptz,
    p_end timestamptz
)
RETURNS TABLE (
    sales_total numeric,
    theoretical_cogs numeric,
    actual_cogs numeric,
    waste_cost numeric,
    food_cost_pct_theo numeric,
    food_cost_pct_actual numeric,
    snapshot_count bigint
)
LANGUAGE sql
STABLE
AS $$
    SELECT
        COALESCE(SUM(f.sales_total), 0) AS sales_total,
        COALESCE(SUM(f.theoretical_cogs), 0) AS theoretical_cogs,
        COALESCE(SUM(f.actual_cogs), 0) AS actual_cogs,
        COALESCE(SUM(f.waste_cost), 0) AS waste_cost,
        CASE WHEN COALESCE(SUM(f.sales_total), 0) > 0
             THEN COALESCE(SUM(f.theoretical_cogs), 0) / SUM(f.sales_total) * 100
             ELSE 0 END AS food_cost_pct_theo,
        CASE WHEN COALESCE(SUM(f.sales_total), 0) > 0
             THEN COALESCE(SUM(f.actual_cogs), 0) / SUM(f.sales_total) * 100
             ELSE 0 END AS food_cost_pct_actual,
        COUNT(*)::bigint AS snapshot_count
    FROM food_cost_snapshots f
    WHERE f.company_id = p_company_id
      AND f.branch_id = p_branch_id
      AND f.period_start >= p_start
      AND f.period_start < p_end;
$$;

CREATE OR REPLACE FUNCTION sp_top_waste(
    p_company_id uuid,
    p_branch_id uuid,
    p_start timestamptz,
    p_end timestamptz,
    p_limit int DEFAULT 20
)
RETURNS TABLE (
    product_id uuid,
    product_name text,
    events bigint,
    qty numeric,
    total_cost numeric
)
LANGUAGE sql
STABLE
AS $$
    SELECT
        p.id AS product_id,
        p.name::text AS product_name,
        COUNT(*)::bigint AS events,
        COALESCE(SUM(w.quantity), 0) AS qty,
        COALESCE(SUM(w.total_cost), 0) AS total_cost
    FROM waste_events w
    INNER JOIN products p ON p.id = w.product_id
    WHERE w.company_id = p_company_id
      AND w.branch_id = p_branch_id
      AND w.created_at >= p_start
      AND w.created_at < p_end
    GROUP BY p.id, p.name
    ORDER BY total_cost DESC
    LIMIT GREATEST(COALESCE(p_limit, 20), 1);
$$;

CREATE OR REPLACE FUNCTION sp_purchase_analysis(
    p_company_id uuid,
    p_branch_id uuid,
    p_start timestamptz,
    p_end timestamptz
)
RETURNS TABLE (
    po_count bigint,
    po_total numeric,
    receipt_count bigint,
    open_po_count bigint,
    overdue_po_count bigint,
    avg_lead_days numeric
)
LANGUAGE sql
STABLE
AS $$
    SELECT
        COUNT(*)::bigint AS po_count,
        COALESCE(SUM(po.total), 0) AS po_total,
        (
            SELECT COUNT(*)::bigint
            FROM goods_receipts gr
            WHERE gr.company_id = p_company_id
              AND gr.branch_id = p_branch_id
              AND gr.received_at >= p_start
              AND gr.received_at < p_end
        ) AS receipt_count,
        COUNT(*) FILTER (WHERE po.status IN ('Draft', 'PendingApproval', 'Approved', 'Sent', 'PartiallyReceived'))::bigint AS open_po_count,
        COUNT(*) FILTER (
            WHERE po.expected_delivery IS NOT NULL
              AND po.expected_delivery < (NOW() AT TIME ZONE 'utc')
              AND po.status NOT IN ('Closed', 'Cancelled', 'FullyReceived', 'Returned', 'Audited')
        )::bigint AS overdue_po_count,
        COALESCE(AVG(
            CASE
                WHEN gr2.received_at IS NOT NULL AND po.order_date IS NOT NULL
                     AND gr2.received_at >= po.order_date
                THEN EXTRACT(EPOCH FROM (gr2.received_at - po.order_date)) / 86400.0
                ELSE NULL
            END
        ), 0) AS avg_lead_days
    FROM purchase_orders po
    LEFT JOIN LATERAL (
        SELECT MIN(gr.received_at) AS received_at
        FROM goods_receipts gr
        WHERE gr.purchase_order_id = po.id
    ) gr2 ON TRUE
    WHERE po.company_id = p_company_id
      AND po.branch_id = p_branch_id
      AND po.order_date >= p_start
      AND po.order_date < p_end;
$$;

CREATE OR REPLACE FUNCTION sp_supplier_analysis(
    p_company_id uuid,
    p_branch_id uuid,
    p_start timestamptz,
    p_end timestamptz
)
RETURNS TABLE (
    supplier_id uuid,
    supplier_name text,
    po_count bigint,
    po_total numeric,
    receipt_count bigint
)
LANGUAGE sql
STABLE
AS $$
    SELECT
        s.id AS supplier_id,
        s.name::text AS supplier_name,
        COUNT(DISTINCT po.id)::bigint AS po_count,
        COALESCE(SUM(po.total), 0) AS po_total,
        COUNT(DISTINCT gr.id)::bigint AS receipt_count
    FROM suppliers s
    LEFT JOIN purchase_orders po
        ON po.supplier_id = s.id
       AND po.company_id = p_company_id
       AND po.branch_id = p_branch_id
       AND po.order_date >= p_start
       AND po.order_date < p_end
    LEFT JOIN goods_receipts gr
        ON gr.purchase_order_id = po.id
    WHERE s.company_id = p_company_id
      AND s.status <> 'Inactive'
    GROUP BY s.id, s.name
    HAVING COUNT(DISTINCT po.id) > 0
    ORDER BY po_total DESC;
$$;

CREATE OR REPLACE FUNCTION sp_profitability(
    p_company_id uuid,
    p_branch_id uuid,
    p_start timestamptz,
    p_end timestamptz
)
RETURNS TABLE (
    revenue numeric,
    cogs_estimate numeric,
    gross_profit numeric,
    gross_margin_pct numeric,
    item_count bigint
)
LANGUAGE sql
STABLE
AS $$
    SELECT
        COALESCE(SUM(oi.quantity * oi.unit_price - COALESCE(oi.discount, 0)), 0) AS revenue,
        COALESCE(SUM(oi.quantity * COALESCE(oi.theoretical_unit_cost, p.average_cost, p.cost, 0)), 0) AS cogs_estimate,
        COALESCE(SUM(oi.quantity * oi.unit_price - COALESCE(oi.discount, 0)), 0)
          - COALESCE(SUM(oi.quantity * COALESCE(oi.theoretical_unit_cost, p.average_cost, p.cost, 0)), 0) AS gross_profit,
        CASE
            WHEN COALESCE(SUM(oi.quantity * oi.unit_price - COALESCE(oi.discount, 0)), 0) > 0
            THEN (
                COALESCE(SUM(oi.quantity * oi.unit_price - COALESCE(oi.discount, 0)), 0)
                - COALESCE(SUM(oi.quantity * COALESCE(oi.theoretical_unit_cost, p.average_cost, p.cost, 0)), 0)
            ) / SUM(oi.quantity * oi.unit_price - COALESCE(oi.discount, 0)) * 100
            ELSE 0
        END AS gross_margin_pct,
        COUNT(oi.id)::bigint AS item_count
    FROM order_items oi
    INNER JOIN orders o ON o.id = oi.order_id
    INNER JOIN products p ON p.id = oi.product_id
    WHERE o.""CompanyId"" = p_company_id
      AND o.""BranchId"" = p_branch_id
      AND o.status = 'Completed'
      AND o.closed_at IS NOT NULL
      AND o.closed_at >= p_start
      AND o.closed_at < p_end;
$$;

CREATE OR REPLACE FUNCTION sp_branch_comparison(
    p_company_id uuid,
    p_start timestamptz,
    p_end timestamptz
)
RETURNS TABLE (
    branch_id uuid,
    branch_name text,
    order_count bigint,
    revenue numeric,
    avg_ticket numeric
)
LANGUAGE sql
STABLE
AS $$
    SELECT
        b.id AS branch_id,
        b.name::text AS branch_name,
        COUNT(o.id) FILTER (WHERE o.status = 'Completed')::bigint AS order_count,
        COALESCE(SUM(o.total_amount) FILTER (WHERE o.status = 'Completed'), 0) AS revenue,
        CASE
            WHEN COUNT(o.id) FILTER (WHERE o.status = 'Completed') > 0
            THEN COALESCE(SUM(o.total_amount) FILTER (WHERE o.status = 'Completed'), 0)
                 / COUNT(o.id) FILTER (WHERE o.status = 'Completed')
            ELSE 0
        END AS avg_ticket
    FROM branches b
    LEFT JOIN orders o
        ON o.""BranchId"" = b.id
       AND o.""CompanyId"" = p_company_id
       AND o.closed_at IS NOT NULL
       AND o.closed_at >= p_start
       AND o.closed_at < p_end
    WHERE b.company_id = p_company_id
    GROUP BY b.id, b.name
    ORDER BY revenue DESC;
$$;

CREATE OR REPLACE FUNCTION sp_executive_dashboard(
    p_company_id uuid,
    p_branch_id uuid,
    p_start timestamptz,
    p_end timestamptz
)
RETURNS TABLE (
    revenue numeric,
    orders_completed bigint,
    avg_ticket numeric,
    gross_margin_pct numeric,
    cash_variance numeric,
    low_stock_count bigint,
    waste_cost numeric,
    open_po_count bigint
)
LANGUAGE sql
STABLE
AS $$
    SELECT
        s.revenue,
        s.completed_count AS orders_completed,
        s.avg_ticket,
        COALESCE(p.gross_margin_pct, 0) AS gross_margin_pct,
        COALESCE(c.total_variance, 0) AS cash_variance,
        COALESCE(i.low_stock_count, 0) AS low_stock_count,
        COALESCE(w.waste_cost, 0) AS waste_cost,
        COALESCE(pu.open_po_count, 0) AS open_po_count
    FROM sp_sales_summary(p_company_id, p_branch_id, p_start, p_end) s
    CROSS JOIN LATERAL sp_profitability(p_company_id, p_branch_id, p_start, p_end) p
    CROSS JOIN LATERAL sp_cash_summary(p_company_id, p_branch_id, p_start, p_end) c
    CROSS JOIN LATERAL sp_inventory_health(p_company_id, p_branch_id) i
    CROSS JOIN LATERAL (
        SELECT COALESCE(SUM(we.total_cost), 0) AS waste_cost
        FROM waste_events we
        WHERE we.company_id = p_company_id
          AND we.branch_id = p_branch_id
          AND we.created_at >= p_start
          AND we.created_at < p_end
    ) w
    CROSS JOIN LATERAL (
        SELECT COUNT(*)::bigint AS open_po_count
        FROM purchase_orders po
        WHERE po.company_id = p_company_id
          AND po.branch_id = p_branch_id
          AND po.status NOT IN ('Closed', 'Cancelled', 'FullyReceived', 'Returned', 'Audited')
    ) pu;
$$;
");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
DROP FUNCTION IF EXISTS sp_executive_dashboard(uuid, uuid, timestamptz, timestamptz);
DROP FUNCTION IF EXISTS sp_branch_comparison(uuid, timestamptz, timestamptz);
DROP FUNCTION IF EXISTS sp_profitability(uuid, uuid, timestamptz, timestamptz);
DROP FUNCTION IF EXISTS sp_supplier_analysis(uuid, uuid, timestamptz, timestamptz);
DROP FUNCTION IF EXISTS sp_purchase_analysis(uuid, uuid, timestamptz, timestamptz);
DROP FUNCTION IF EXISTS sp_top_waste(uuid, uuid, timestamptz, timestamptz, int);
DROP FUNCTION IF EXISTS sp_food_cost_summary(uuid, uuid, timestamptz, timestamptz);
DROP FUNCTION IF EXISTS sp_inventory_health(uuid, uuid);
DROP FUNCTION IF EXISTS sp_cash_summary(uuid, uuid, timestamptz, timestamptz);
DROP FUNCTION IF EXISTS sp_station_performance(uuid, uuid, timestamptz, timestamptz);
DROP FUNCTION IF EXISTS sp_waiter_performance(uuid, uuid, timestamptz, timestamptz);
DROP FUNCTION IF EXISTS sp_top_products(uuid, uuid, timestamptz, timestamptz, int);
DROP FUNCTION IF EXISTS sp_hourly_sales(uuid, uuid, timestamptz, timestamptz);
DROP FUNCTION IF EXISTS sp_sales_summary(uuid, uuid, timestamptz, timestamptz);
DROP INDEX IF EXISTS ""IX_orders_branch_closed"";
DROP INDEX IF EXISTS ""IX_inv_mov_branch_created"";
");
    }
}