# 04 — Stored Procedure Design

PostgreSQL implements enterprise “SPs” as **STABLE SQL functions**. Source of truth: `Sql/Bi/01_native_bi_functions.sql`. Applied by migration `20260730190000_NativeBiAnalyticsLayer`.

## Design rules

1. Hard filter `p_company_id` (+ `p_branch_id` except company-wide branch comparison).
2. Prefer set-based SQL; no N+1; no cursors.
3. Quote PascalCase columns (`"CompanyId"`, `"BranchId"` on orders/payments/inventory_movements).
4. Sales window on `orders.closed_at` for completed tickets.
5. Cost estimate: `COALESCE(theoretical_unit_cost, average_cost, cost, 0)`.

## Catalog

| Function | Business question | Key tables |
|----------|-------------------|------------|
| `sp_sales_summary` | ¿Cuánto vendimos / ticket / cancelaciones? | orders |
| `sp_hourly_sales` | ¿En qué horas se vende? | orders |
| `sp_top_products` | ¿Qué productos generan venta y margen? | order_items, products |
| `sp_waiter_performance` | ¿Cómo rinde cada mesero? | orders, users |
| `sp_station_performance` | ¿Cómo rinde cada estación y prep time? | stations, order_items |
| `sp_cash_summary` | ¿Sesiones, ventas caja, varianza? | cash_sessions |
| `sp_inventory_health` | ¿Stock bajo / valor / merma 30d? | products, inventory_movements |
| `sp_food_cost_summary` | ¿FC% teórico/actual del periodo? | food_cost_snapshots |
| `sp_top_waste` | ¿Dónde está la merma? | waste_events |
| `sp_purchase_analysis` | ¿PO, lead time, overdue? | purchase_orders, goods_receipts |
| `sp_supplier_analysis` | ¿Gasto por proveedor? | suppliers, POs, receipts |
| `sp_profitability` | ¿Utilidad bruta estimada? | order_items, products |
| `sp_branch_comparison` | ¿Comparar sucursales? | branches, orders |
| `sp_executive_dashboard` | ¿KPIs dueño en un call? | composes above |

## App entry points

- Service: `Services/Intelligence/BiNativeAnalyticsService.cs`
- API/UI: `Controllers/BiNativeController.cs`, `Views/BiNative/Index.cshtml`
- DI: `AddEnterpriseIntelligenceModule` → `IBiNativeAnalyticsService`

## Not implemented as SP (data missing or better in app)

| Requested | Reason |
|-----------|--------|
| SLA delay kitchen | No prep SLA target table — **NO DISPONIBLE** |
| Customer retention cohorts | Sparse customer_id + no visit ledger — **NO DISPONIBLE** |
| Stock point-in-time history | No snapshot table — **NO DISPONIBLE** |
| Payment method dimension | String only — use `GROUP BY method` in app |

## Indexes added with SPs

- `IX_orders_branch_closed` (partial `closed_at IS NOT NULL`)
- `IX_inv_mov_branch_created`
