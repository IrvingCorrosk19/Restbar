using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestBar.Migrations;

public partial class AnalyticsEnterpriseSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"-- RB-025 Enterprise Analytics layer
-- Schema: analytics
-- Wraps/extends public.sp_* with consistent names + additional decision-oriented functions.
-- Idempotent.

CREATE SCHEMA IF NOT EXISTS analytics;

-- ========== VIEWS (thin, tenant-filtered at query time) ==========

CREATE OR REPLACE VIEW analytics.v_completed_orders AS
SELECT
    o.id,
    o.""CompanyId"" AS company_id,
    o.""BranchId"" AS branch_id,
    o.table_id,
    o.user_id,
    o.customer_id,
    o.status,
    o.total_amount,
    o.discount_amount,
    o.opened_at,
    o.closed_at,
    o.""CreatedAt"" AS created_at
FROM orders o
WHERE o.status = 'Completed'
  AND o.closed_at IS NOT NULL;

CREATE OR REPLACE VIEW analytics.v_order_lines AS
SELECT
    oi.id,
    oi.order_id,
    o.""CompanyId"" AS company_id,
    o.""BranchId"" AS branch_id,
    oi.product_id,
    oi.quantity,
    oi.unit_price,
    COALESCE(oi.discount, 0) AS discount,
    (oi.quantity * oi.unit_price - COALESCE(oi.discount, 0)) AS line_revenue,
    COALESCE(oi.theoretical_unit_cost, p.average_cost, p.cost, 0) AS unit_cost,
    oi.quantity * COALESCE(oi.theoretical_unit_cost, p.average_cost, p.cost, 0) AS line_cogs,
    oi.sent_at,
    oi.prepared_at,
    oi.prepared_by_station_id,
    o.closed_at,
    o.status AS order_status,
    p.category_id,
    p.name AS product_name
FROM order_items oi
INNER JOIN orders o ON o.id = oi.order_id
INNER JOIN products p ON p.id = oi.product_id;

-- ========== WRAPPERS around public functions (single KPI source) ==========

CREATE OR REPLACE FUNCTION analytics.sp_sales_summary(p_company_id uuid, p_branch_id uuid, p_start timestamptz, p_end timestamptz)
RETURNS TABLE (order_count bigint, revenue numeric, avg_ticket numeric, cancelled_count bigint, discount_total numeric, completed_count bigint)
LANGUAGE sql STABLE AS $$ SELECT * FROM public.sp_sales_summary(p_company_id, p_branch_id, p_start, p_end); $$;

CREATE OR REPLACE FUNCTION analytics.sp_sales_by_hour(p_company_id uuid, p_branch_id uuid, p_start timestamptz, p_end timestamptz)
RETURNS TABLE (sale_hour int, order_count bigint, revenue numeric)
LANGUAGE sql STABLE AS $$ SELECT * FROM public.sp_hourly_sales(p_company_id, p_branch_id, p_start, p_end); $$;

CREATE OR REPLACE FUNCTION analytics.sp_sales_by_product(p_company_id uuid, p_branch_id uuid, p_start timestamptz, p_end timestamptz, p_limit int DEFAULT 50)
RETURNS TABLE (product_id uuid, product_name text, qty_sold numeric, revenue numeric, cogs_estimate numeric, margin_estimate numeric)
LANGUAGE sql STABLE AS $$ SELECT * FROM public.sp_top_products(p_company_id, p_branch_id, p_start, p_end, p_limit); $$;

CREATE OR REPLACE FUNCTION analytics.sp_waiter_performance(p_company_id uuid, p_branch_id uuid, p_start timestamptz, p_end timestamptz)
RETURNS TABLE (user_id uuid, waiter_name text, order_count bigint, revenue numeric, avg_ticket numeric)
LANGUAGE sql STABLE AS $$ SELECT * FROM public.sp_waiter_performance(p_company_id, p_branch_id, p_start, p_end); $$;

CREATE OR REPLACE FUNCTION analytics.sp_station_performance(p_company_id uuid, p_branch_id uuid, p_start timestamptz, p_end timestamptz)
RETURNS TABLE (station_id uuid, station_name text, items_processed bigint, orders_processed bigint, avg_prep_minutes numeric, revenue numeric)
LANGUAGE sql STABLE AS $$ SELECT * FROM public.sp_station_performance(p_company_id, p_branch_id, p_start, p_end); $$;

CREATE OR REPLACE FUNCTION analytics.sp_cash_summary(p_company_id uuid, p_branch_id uuid, p_start timestamptz, p_end timestamptz)
RETURNS TABLE (sessions_opened bigint, sessions_closed bigint, total_sales numeric, total_refunds numeric, total_paid_in numeric, total_paid_out numeric, total_variance numeric, abs_variance numeric)
LANGUAGE sql STABLE AS $$ SELECT * FROM public.sp_cash_summary(p_company_id, p_branch_id, p_start, p_end); $$;

CREATE OR REPLACE FUNCTION analytics.sp_inventory_health(p_company_id uuid, p_branch_id uuid)
RETURNS TABLE (tracked_products bigint, low_stock_count bigint, zero_stock_count bigint, stock_value_estimate numeric, waste_qty_30d numeric, waste_cost_30d numeric, sale_movements_30d bigint)
LANGUAGE sql STABLE AS $$ SELECT * FROM public.sp_inventory_health(p_company_id, p_branch_id); $$;

CREATE OR REPLACE FUNCTION analytics.sp_food_cost_summary(p_company_id uuid, p_branch_id uuid, p_start timestamptz, p_end timestamptz)
RETURNS TABLE (sales_total numeric, theoretical_cogs numeric, actual_cogs numeric, waste_cost numeric, food_cost_pct_theo numeric, food_cost_pct_actual numeric, snapshot_count bigint)
LANGUAGE sql STABLE AS $$ SELECT * FROM public.sp_food_cost_summary(p_company_id, p_branch_id, p_start, p_end); $$;

CREATE OR REPLACE FUNCTION analytics.sp_waste_analysis(p_company_id uuid, p_branch_id uuid, p_start timestamptz, p_end timestamptz, p_limit int DEFAULT 50)
RETURNS TABLE (product_id uuid, product_name text, events bigint, qty numeric, total_cost numeric)
LANGUAGE sql STABLE AS $$ SELECT * FROM public.sp_top_waste(p_company_id, p_branch_id, p_start, p_end, p_limit); $$;

CREATE OR REPLACE FUNCTION analytics.sp_purchase_summary(p_company_id uuid, p_branch_id uuid, p_start timestamptz, p_end timestamptz)
RETURNS TABLE (po_count bigint, po_total numeric, receipt_count bigint, open_po_count bigint, overdue_po_count bigint, avg_lead_days numeric)
LANGUAGE sql STABLE AS $$ SELECT * FROM public.sp_purchase_analysis(p_company_id, p_branch_id, p_start, p_end); $$;

CREATE OR REPLACE FUNCTION analytics.sp_supplier_performance(p_company_id uuid, p_branch_id uuid, p_start timestamptz, p_end timestamptz)
RETURNS TABLE (supplier_id uuid, supplier_name text, po_count bigint, po_total numeric, receipt_count bigint)
LANGUAGE sql STABLE AS $$ SELECT * FROM public.sp_supplier_analysis(p_company_id, p_branch_id, p_start, p_end); $$;

CREATE OR REPLACE FUNCTION analytics.sp_product_profitability(p_company_id uuid, p_branch_id uuid, p_start timestamptz, p_end timestamptz)
RETURNS TABLE (revenue numeric, cogs_estimate numeric, gross_profit numeric, gross_margin_pct numeric, item_count bigint)
LANGUAGE sql STABLE AS $$ SELECT * FROM public.sp_profitability(p_company_id, p_branch_id, p_start, p_end); $$;

CREATE OR REPLACE FUNCTION analytics.sp_branch_comparison(p_company_id uuid, p_start timestamptz, p_end timestamptz)
RETURNS TABLE (branch_id uuid, branch_name text, order_count bigint, revenue numeric, avg_ticket numeric)
LANGUAGE sql STABLE AS $$ SELECT * FROM public.sp_branch_comparison(p_company_id, p_start, p_end); $$;

CREATE OR REPLACE FUNCTION analytics.sp_executive_summary(p_company_id uuid, p_branch_id uuid, p_start timestamptz, p_end timestamptz)
RETURNS TABLE (revenue numeric, orders_completed bigint, avg_ticket numeric, gross_margin_pct numeric, cash_variance numeric, low_stock_count bigint, waste_cost numeric, open_po_count bigint)
LANGUAGE sql STABLE AS $$ SELECT * FROM public.sp_executive_dashboard(p_company_id, p_branch_id, p_start, p_end); $$;

-- ========== NEW ANALYTICS FUNCTIONS ==========

CREATE OR REPLACE FUNCTION analytics.sp_sales_by_category(
    p_company_id uuid, p_branch_id uuid, p_start timestamptz, p_end timestamptz
)
RETURNS TABLE (category_id uuid, category_name text, qty_sold numeric, revenue numeric, cogs_estimate numeric, margin_estimate numeric)
LANGUAGE sql STABLE AS $$
    SELECT
        COALESCE(c.id, '00000000-0000-0000-0000-000000000000'::uuid) AS category_id,
        COALESCE(c.name, '(Sin categoría)')::text AS category_name,
        COALESCE(SUM(l.quantity), 0) AS qty_sold,
        COALESCE(SUM(l.line_revenue), 0) AS revenue,
        COALESCE(SUM(l.line_cogs), 0) AS cogs_estimate,
        COALESCE(SUM(l.line_revenue), 0) - COALESCE(SUM(l.line_cogs), 0) AS margin_estimate
    FROM analytics.v_order_lines l
    LEFT JOIN categories c ON c.id = l.category_id
    WHERE l.company_id = p_company_id
      AND l.branch_id = p_branch_id
      AND l.order_status = 'Completed'
      AND l.closed_at >= p_start AND l.closed_at < p_end
    GROUP BY c.id, c.name
    ORDER BY 4 DESC;
$$;

CREATE OR REPLACE FUNCTION analytics.sp_sales_by_payment(
    p_company_id uuid, p_branch_id uuid, p_start timestamptz, p_end timestamptz
)
RETURNS TABLE (payment_method text, payment_count bigint, amount numeric, tip_amount numeric)
LANGUAGE sql STABLE AS $$
    SELECT
        COALESCE(NULLIF(TRIM(p.method), ''), '(Sin método)')::text AS payment_method,
        COUNT(*)::bigint AS payment_count,
        COALESCE(SUM(p.amount), 0) AS amount,
        COALESCE(SUM(p.tip_amount), 0) AS tip_amount
    FROM payments p
    WHERE p.""CompanyId"" = p_company_id
      AND p.""BranchId"" = p_branch_id
      AND COALESCE(p.is_voided, false) = false
      AND p.paid_at >= p_start AND p.paid_at < p_end
    GROUP BY 1
    ORDER BY amount DESC;
$$;

CREATE OR REPLACE FUNCTION analytics.sp_sales_trend(
    p_company_id uuid, p_branch_id uuid, p_start timestamptz, p_end timestamptz, p_grain text DEFAULT 'day'
)
RETURNS TABLE (bucket_start timestamptz, order_count bigint, revenue numeric, avg_ticket numeric)
LANGUAGE sql STABLE AS $$
    SELECT
        date_trunc(CASE WHEN lower(p_grain) IN ('hour','day','week','month') THEN lower(p_grain) ELSE 'day' END, o.closed_at) AS bucket_start,
        COUNT(*)::bigint AS order_count,
        COALESCE(SUM(o.total_amount), 0) AS revenue,
        CASE WHEN COUNT(*) > 0 THEN COALESCE(SUM(o.total_amount), 0) / COUNT(*) ELSE 0 END AS avg_ticket
    FROM orders o
    WHERE o.""CompanyId"" = p_company_id
      AND o.""BranchId"" = p_branch_id
      AND o.status = 'Completed'
      AND o.closed_at IS NOT NULL
      AND o.closed_at >= p_start AND o.closed_at < p_end
    GROUP BY 1
    ORDER BY 1;
$$;

CREATE OR REPLACE FUNCTION analytics.sp_period_comparison(
    p_company_id uuid, p_branch_id uuid,
    p_start timestamptz, p_end timestamptz,
    p_comp_start timestamptz, p_comp_end timestamptz
)
RETURNS TABLE (
    metric text,
    current_value numeric,
    previous_value numeric,
    abs_change numeric,
    pct_change numeric
)
LANGUAGE sql STABLE AS $$
    WITH cur AS (
        SELECT * FROM analytics.sp_sales_summary(p_company_id, p_branch_id, p_start, p_end)
    ),
    prev AS (
        SELECT * FROM analytics.sp_sales_summary(p_company_id, p_branch_id, p_comp_start, p_comp_end)
    )
    SELECT m.metric, m.current_value, m.previous_value,
           m.current_value - m.previous_value AS abs_change,
           CASE WHEN m.previous_value = 0 THEN NULL
                ELSE ROUND(((m.current_value - m.previous_value) / ABS(m.previous_value)) * 100, 2)
           END AS pct_change
    FROM (
        SELECT 'revenue'::text, cur.revenue, prev.revenue FROM cur, prev
        UNION ALL
        SELECT 'orders', cur.completed_count::numeric, prev.completed_count::numeric FROM cur, prev
        UNION ALL
        SELECT 'avg_ticket', cur.avg_ticket, prev.avg_ticket FROM cur, prev
        UNION ALL
        SELECT 'discounts', cur.discount_total, prev.discount_total FROM cur, prev
        UNION ALL
        SELECT 'cancelled', cur.cancelled_count::numeric, prev.cancelled_count::numeric FROM cur, prev
    ) AS m(metric, current_value, previous_value);
$$;

CREATE OR REPLACE FUNCTION analytics.sp_cash_variance(
    p_company_id uuid, p_branch_id uuid, p_start timestamptz, p_end timestamptz
)
RETURNS TABLE (
    session_id uuid,
    session_number int,
    register_code text,
    opened_at timestamptz,
    closed_at timestamptz,
    status text,
    expected_cash numeric,
    counted_cash numeric,
    variance numeric,
    opened_by uuid
)
LANGUAGE sql STABLE AS $$
    SELECT
        cs.id,
        cs.session_number,
        COALESCE(cr.code, '')::text,
        cs.opened_at,
        cs.closed_at,
        cs.status::text,
        COALESCE(cs.expected_cash, 0),
        COALESCE(cs.counted_cash, 0),
        COALESCE(cs.variance, 0),
        cs.opened_by_user_id
    FROM cash_sessions cs
    LEFT JOIN cash_registers cr ON cr.id = cs.cash_register_id
    WHERE cs.company_id = p_company_id
      AND cs.branch_id = p_branch_id
      AND cs.opened_at >= p_start AND cs.opened_at < p_end
    ORDER BY ABS(COALESCE(cs.variance, 0)) DESC, cs.opened_at DESC;
$$;

CREATE OR REPLACE FUNCTION analytics.sp_inventory_turnover(
    p_company_id uuid, p_branch_id uuid, p_days int DEFAULT 30
)
RETURNS TABLE (
    product_id uuid,
    product_name text,
    stock numeric,
    consumed_qty numeric,
    turnover_ratio numeric,
    coverage_days numeric
)
LANGUAGE sql STABLE AS $$
    WITH cons AS (
        SELECT im.product_id,
               COALESCE(SUM(im.quantity) FILTER (WHERE im.movement_type = 'Sale'), 0) AS consumed_qty
        FROM inventory_movements im
        WHERE im.""CompanyId"" = p_company_id
          AND (im.""BranchId"" IS NULL OR im.""BranchId"" = p_branch_id)
          AND im.created_at >= (NOW() AT TIME ZONE 'utc') - make_interval(days => GREATEST(p_days, 1))
        GROUP BY im.product_id
    )
    SELECT
        p.id,
        p.name::text,
        COALESCE(p.stock, 0),
        COALESCE(c.consumed_qty, 0),
        CASE WHEN COALESCE(p.stock, 0) > 0
             THEN COALESCE(c.consumed_qty, 0) / p.stock
             ELSE NULL END AS turnover_ratio,
        CASE WHEN COALESCE(c.consumed_qty, 0) > 0
             THEN (COALESCE(p.stock, 0) / (c.consumed_qty / GREATEST(p_days, 1)))
             ELSE NULL END AS coverage_days
    FROM products p
    LEFT JOIN cons c ON c.product_id = p.id
    WHERE p.company_id = p_company_id
      AND (p.branch_id IS NULL OR p.branch_id = p_branch_id)
      AND p.track_inventory = true
      AND p.is_active = true
    ORDER BY coverage_days NULLS FIRST, consumed_qty DESC
    LIMIT 200;
$$;

CREATE OR REPLACE FUNCTION analytics.sp_inventory_coverage(
    p_company_id uuid, p_branch_id uuid, p_days int DEFAULT 30
)
RETURNS TABLE (product_id uuid, product_name text, stock numeric, daily_consumption numeric, coverage_days numeric, is_critical boolean)
LANGUAGE sql STABLE AS $$
    SELECT
        t.product_id, t.product_name, t.stock,
        CASE WHEN GREATEST(p_days, 1) > 0 THEN t.consumed_qty / GREATEST(p_days, 1) ELSE 0 END,
        t.coverage_days,
        (t.stock <= COALESCE((SELECT min_stock FROM products p2 WHERE p2.id = t.product_id), 0)
         OR (t.coverage_days IS NOT NULL AND t.coverage_days < 3)) AS is_critical
    FROM analytics.sp_inventory_turnover(p_company_id, p_branch_id, p_days) t
    ORDER BY is_critical DESC, coverage_days NULLS FIRST;
$$;

CREATE OR REPLACE FUNCTION analytics.sp_menu_engineering(
    p_company_id uuid, p_branch_id uuid, p_start timestamptz, p_end timestamptz
)
RETURNS TABLE (
    product_id uuid,
    product_name text,
    qty_sold numeric,
    revenue numeric,
    margin_estimate numeric,
    margin_pct numeric,
    classification text
)
LANGUAGE sql STABLE AS $$
    WITH base AS (
        SELECT * FROM analytics.sp_sales_by_product(p_company_id, p_branch_id, p_start, p_end, 500)
    ),
    stats AS (
        SELECT
            PERCENTILE_CONT(0.5) WITHIN GROUP (ORDER BY qty_sold) AS med_qty,
            PERCENTILE_CONT(0.5) WITHIN GROUP (ORDER BY
                CASE WHEN revenue > 0 THEN margin_estimate / revenue ELSE 0 END) AS med_margin_pct
        FROM base
    )
    SELECT
        b.product_id,
        b.product_name,
        b.qty_sold,
        b.revenue,
        b.margin_estimate,
        CASE WHEN b.revenue > 0 THEN ROUND((b.margin_estimate / b.revenue) * 100, 2) ELSE 0 END AS margin_pct,
        CASE
            WHEN b.qty_sold >= s.med_qty AND (CASE WHEN b.revenue > 0 THEN b.margin_estimate / b.revenue ELSE 0 END) >= s.med_margin_pct
                THEN 'Star'
            WHEN b.qty_sold >= s.med_qty AND (CASE WHEN b.revenue > 0 THEN b.margin_estimate / b.revenue ELSE 0 END) < s.med_margin_pct
                THEN 'Plowhorse'
            WHEN b.qty_sold < s.med_qty AND (CASE WHEN b.revenue > 0 THEN b.margin_estimate / b.revenue ELSE 0 END) >= s.med_margin_pct
                THEN 'Puzzle'
            ELSE 'Dog'
        END AS classification
    FROM base b CROSS JOIN stats s
    ORDER BY b.revenue DESC;
$$;

CREATE OR REPLACE FUNCTION analytics.sp_kitchen_performance(
    p_company_id uuid, p_branch_id uuid, p_start timestamptz, p_end timestamptz
)
RETURNS TABLE (
    items_with_prep bigint,
    avg_prep_minutes numeric,
    p90_prep_minutes numeric,
    delayed_items_gt_20m bigint
)
LANGUAGE sql STABLE AS $$
    WITH prep AS (
        SELECT EXTRACT(EPOCH FROM (l.prepared_at - l.sent_at)) / 60.0 AS mins
        FROM analytics.v_order_lines l
        WHERE l.company_id = p_company_id
          AND l.branch_id = p_branch_id
          AND l.order_status = 'Completed'
          AND l.closed_at >= p_start AND l.closed_at < p_end
          AND l.sent_at IS NOT NULL
          AND l.prepared_at IS NOT NULL
          AND l.prepared_at >= l.sent_at
    )
    SELECT
        COUNT(*)::bigint,
        COALESCE(AVG(mins), 0),
        COALESCE(PERCENTILE_CONT(0.9) WITHIN GROUP (ORDER BY mins), 0),
        COUNT(*) FILTER (WHERE mins > 20)::bigint
    FROM prep;
$$;

CREATE OR REPLACE FUNCTION analytics.sp_table_turnover(
    p_company_id uuid, p_branch_id uuid, p_start timestamptz, p_end timestamptz
)
RETURNS TABLE (
    table_id uuid,
    table_number text,
    order_count bigint,
    revenue numeric,
    avg_dwell_minutes numeric
)
LANGUAGE sql STABLE AS $$
    SELECT
        t.id,
        COALESCE(t.table_number::text, t.id::text),
        COUNT(o.id)::bigint,
        COALESCE(SUM(o.total_amount), 0),
        COALESCE(AVG(EXTRACT(EPOCH FROM (o.closed_at - o.opened_at)) / 60.0)
            FILTER (WHERE o.opened_at IS NOT NULL AND o.closed_at IS NOT NULL), 0)
    FROM tables t
    LEFT JOIN orders o
        ON o.table_id = t.id
       AND o.""CompanyId"" = p_company_id
       AND o.""BranchId"" = p_branch_id
       AND o.status = 'Completed'
       AND o.closed_at >= p_start AND o.closed_at < p_end
    WHERE t.company_id = p_company_id
      AND (t.branch_id IS NULL OR t.branch_id = p_branch_id)
    GROUP BY t.id, t.table_number
    ORDER BY 4 DESC;
$$;

CREATE OR REPLACE FUNCTION analytics.sp_sales_by_branch(
    p_company_id uuid, p_start timestamptz, p_end timestamptz
)
RETURNS TABLE (branch_id uuid, branch_name text, order_count bigint, revenue numeric, avg_ticket numeric)
LANGUAGE sql STABLE AS $$
    SELECT * FROM analytics.sp_branch_comparison(p_company_id, p_start, p_end);
$$;

CREATE OR REPLACE FUNCTION analytics.sp_supplier_price_variation(
    p_company_id uuid, p_start timestamptz, p_end timestamptz
)
RETURNS TABLE (
    product_id uuid,
    product_name text,
    supplier_id uuid,
    supplier_name text,
    first_cost numeric,
    last_cost numeric,
    pct_change numeric,
    samples bigint
)
LANGUAGE sql STABLE AS $$
    WITH hist AS (
        SELECT
            ph.product_id,
            ph.supplier_id,
            ph.unit_cost,
            ph.recorded_at,
            FIRST_VALUE(ph.unit_cost) OVER (PARTITION BY ph.product_id, ph.supplier_id ORDER BY ph.recorded_at) AS first_cost,
            FIRST_VALUE(ph.unit_cost) OVER (PARTITION BY ph.product_id, ph.supplier_id ORDER BY ph.recorded_at DESC) AS last_cost
        FROM price_history ph
        WHERE ph.company_id = p_company_id
          AND ph.recorded_at >= p_start AND ph.recorded_at < p_end
    )
    SELECT
        h.product_id,
        p.name::text,
        h.supplier_id,
        COALESCE(s.name, '')::text,
        MIN(h.first_cost),
        MIN(h.last_cost),
        CASE WHEN MIN(h.first_cost) = 0 THEN NULL
             ELSE ROUND(((MIN(h.last_cost) - MIN(h.first_cost)) / ABS(MIN(h.first_cost))) * 100, 2) END,
        COUNT(*)::bigint
    FROM hist h
    INNER JOIN products p ON p.id = h.product_id
    LEFT JOIN suppliers s ON s.id = h.supplier_id
    GROUP BY h.product_id, p.name, h.supplier_id, s.name
    HAVING COUNT(*) >= 2
    ORDER BY 7 DESC NULLS LAST
    LIMIT 200;
$$;
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
DROP SCHEMA IF EXISTS analytics CASCADE;
");
    }
}