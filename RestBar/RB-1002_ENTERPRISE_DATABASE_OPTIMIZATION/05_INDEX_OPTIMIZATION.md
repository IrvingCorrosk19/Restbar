# 05 — Index Optimization (RB-1002)

## Created (idempotent)

| Index | Purpose | Evidence |
|-------|---------|----------|
| `IX_audit_logs_company_timestamp` | Tenant audit lists | OrderBy Timestamp + CompanyId |
| `IX_audit_logs_timestamp` | Global recent | Index Scan confirmed |
| `IX_audit_logs_company_module_timestamp` | Module filters | GetByModuleAsync |
| `IX_audit_logs_company_error_timestamp` (partial) | Error views | GetErrorsAsync |
| `IX_orders_status_opened` | Kitchen board | Seq→Bitmap Index |
| `IX_orders_company_status_opened` | Tenant kitchen/POS | Future MT-scoped queries |
| `IX_orders_branch_closed` / `IX_orders_company_closed` | Closed sales windows | Analytics/reports |
| `IX_order_items_kitchen_status_sent` (partial) | Station pending items | Kitchen item filter |
| `IX_customers_company_name` / `_email` | Search | SearchCustomersAsync |
| `IX_products_company_branch_active` | POS catalog | Active product browse |

## Not dropped

Zero-scan PK indexes on rare tables (recipes, settings) remain — storage trivial; drop only after 30d `pg_stat_user_indexes` review in production.

## Duplicate / redundant review

- `IX_orders_BranchId` + composite `branch_status_opened` both useful (different selectivity).
- CompanyId singleton indexes kept for FK-style filters.

## Apply artifacts

- Migration: `20260731120000_Rb1002PerformanceIndexes`
- SQL: `Sql/Performance/01_rb1002_hot_indexes.sql` (applied on VPS 2026-07-31)
