# 01 — Executive Summary

## Verdict: PASS WITH CONDITIONS

RestBar now includes a **Native Executive Analytics Center** (/ExecutiveAnalytics) backed by PostgreSQL schema `analytics`, shared KPI catalog, HTML reports, and CSV/Excel/PDF exports — without Power BI.

## What owners can decide today

- How much they sold / ticket / cancellations
- Where margin is estimated negative (products)
- Cash variance sessions to audit
- Critical stock and overdue POs
- Kitchen delays (>20m prep) when timestamps exist
- Sales drop vs prior period (≥15%)

## Conditions

1. Apply migrations `NativeBiAnalyticsLayer` + `AnalyticsEnterpriseSchema`.
2. Food Cost % requires generated `food_cost_snapshots`.
3. Prep times require `sent_at`/`prepared_at` population.
4. Taxes, covers/guests, reserved stock, warehouse master, physical count accuracy: **NO DISPONIBLE** (catalogued, not faked).
5. Legacy SalesReport BranchId filter gaps remain outside this module.

## Entry points

- UI: `/ExecutiveAnalytics`
- API: `/api/analytics/*`
- SQL: `Sql/Bi/01_native_bi_functions.sql`, `Sql/Bi/02_analytics_schema.sql`
