# 01 — Database Analysis (Evidence)

**Audit date:** 2026-07-30  
**Scope:** RestBar PostgreSQL + EF Core model (no assumptions)

## Verdict (DB layer)

RestBar has a **rich OLTP schema** covering sales, payments, kitchen, inventory movements, procurement, food cost, cash, and BI snapshot tables.  
It did **not** previously have analytical SQL views, materialized views, or BI stored procedures. RB-025 adds PostgreSQL `sp_*` functions + supporting indexes.

## Evidence sources

| Source | Path |
|--------|------|
| Core context | `Models/RestBarContext.cs` |
| Cash | `Models/RestBarContext.Cash.cs` |
| Procurement | `Models/RestBarContext.Procurement.cs` |
| Food Cost | `Models/RestBarContext.FoodCost.cs` |
| Intelligence BI tables | `Models/RestBarContext.Intelligence.cs`, migration `20260730005815_BusinessIntelligenceEnterprise` |
| Operational indexes | `20260729124307_EnterpriseFoundationOperationalIndexes`, `20260730023951_InventoryQueryPerformanceIndexes` |
| Native BI SQL | `Sql/Bi/01_native_bi_functions.sql`, migration `20260730190000_NativeBiAnalyticsLayer` |

## Table inventory (analytical relevance)

### Sales / orders
- `orders`, `order_items`, `order_cancellation_logs`, `payments`, `payment_refunds`, `split_payments`, `invoices`

### Operations
- `tables`, `areas`, `stations`, `shifts`, `users`, `customers`

### Inventory
- `products`, `product_stock_assignments`, `inventory_movements`, `stock_transfers`, `recipes`, `recipe_lines`

### Procurement
- `suppliers`, `purchase_orders`, `purchase_order_lines`, `goods_receipts`, `goods_receipt_lines`, `price_history`, `supplier_scores`

### Food cost
- `food_cost_snapshots`, `recipe_cost_histories`, `waste_events`, `variance_alerts`

### Cash
- `cash_registers`, `cash_sessions`, `cash_movements`, `cash_counts`, `cash_z_reports`, `cash_audit_events`

### BI store (app-aggregated)
- `executive_snapshots`, `bi_insights`, `bi_alerts`, `bi_scores`, `bi_audit_events`, `forecast_seeds`

## Views / materialized views / analytic procedures (pre-RB-025)

| Object type | Status |
|-------------|--------|
| SQL VIEW | **NO DISPONIBLE** (no `CREATE VIEW` in repo) |
| MATERIALIZED VIEW | **NO DISPONIBLE** |
| Analytic PROCEDURE/FUNCTION | **NO DISPONIBLE** before RB-025 |
| Trigger helpers only | `restbarIIC.sql` (`update_*_updated_at`) — not analytics |

## Indexes relevant to BI filters

| Index | Table | Columns |
|-------|-------|---------|
| `IX_orders_branch_status_opened` | orders | BranchId, status, opened_at |
| `IX_orders_branch_created` | orders | BranchId, CreatedAt |
| `IX_orders_branch_closed` | orders | BranchId, closed_at (partial) — **added RB-025** |
| `IX_payments_branch_paid_at` | payments | BranchId, paid_at |
| `IX_inv_mov_company_created` | inventory_movements | CompanyId, created_at |
| `IX_inv_mov_branch_created` | inventory_movements | BranchId, created_at — **added RB-025** |
| `IX_inv_mov_product_created` | inventory_movements | product_id, created_at |
| BI branch-date indexes | executive_snapshots, bi_* | company/branch scoped |

## Column naming hazard (evidence)

Legacy core tables mix PascalCase tenant columns (`orders."CompanyId"`, `orders."BranchId"`, `payments."BranchId"`, `inventory_movements."CompanyId"`) with snake_case enterprise tables (`cash_sessions.company_id`, `food_cost_snapshots.branch_id`).  
Native SQL **must** quote PascalCase identifiers. Documented in SP design.

## Sufficient for indicators without model change?

**Yes for core restaurant BI facts** (sales, ticket, product mix, waiter, station prep times when timestamps populated, cash variance, PO/receipts, waste events, stock on hand, food-cost snapshots).  

**No for warehouse-grade history** (no lot master, no perpetual stock snapshot table, no payment method dimension table, no customer visit ledger beyond `orders.customer_id`).

## Históricos / auditoría

| Domain | Evidence | Gap |
|--------|----------|-----|
| Order lifecycle | `opened_at`, `closed_at`, status, cancellation logs | Peak-hour pre-aggregation NO DISPONIBLE as table |
| Inventory | `inventory_movements` kardex | No daily stock snapshot table |
| Price | `price_history` | — |
| Cash | hash-chained `cash_movements`, `cash_audit_events` | — |
| Procurement | `procurement_audit_events` | — |
| Food cost | `food_cost_audit_events`, snapshots | Actual COGS depends on snapshot generation |
| App BI | `bi_audit_events`, `executive_snapshots` | Copilot memory is conversational, not fact warehouse |
