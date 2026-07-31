# 09 — Gap Analysis

## P0 — Fix before calling “enterprise BI complete”

| Gap | Evidence | Action |
|-----|----------|--------|
| Legacy sales/advanced reports weak BranchId filters | SalesReportService / AdvancedReportsService | Align with claim branch or remove from BI nav |
| Cost dualidad (Cost vs AverageCost vs TheoreticalUnitCost) | Product + OrderItem | Document single COGS policy; prefer TheoreticalUnitCost when present |
| Food cost snapshots not continuous | food_cost_snapshots only when generated | Schedule snapshot job |
| Kitchen timestamps optional | SentAt/PreparedAt may be null | Enforce KDS write path |

## P1 — Data model captures

| Gap | Status | Capture suggestion |
|-----|--------|--------------------|
| PaymentMethod dimension | NO DISPONIBLE as entity | Optional catalog table; keep string method |
| Stock daily snapshot | NO DISPONIBLE | Nightly `stock_snapshots` if historical coverage KPI is required |
| Prep SLA targets | NO DISPONIBLE | `station_sla_minutes` |
| Customer retention ledger | NO DISPONIBLE | Require customer on close + visit facts |
| Lot / expiry inventory | PARTIAL | Lot master (beyond receipt line) |
| Comprador analytics UI | PARTIAL | Use requested_by_user_id in SP |

## P2 — Productization

| Gap | Notes |
|-----|-------|
| PDF/Excel enterprise exports | Flag `EnableReportExports` largely stubbed |
| Full dark-theme BI chrome | Not blocking |
| Materialized views | Only after EXPLAIN proof |
| External BI connectors | Optional future (views for Power BI if client asks) |

## What is NOT a gap

- Orders/payments/items facts for sales BI
- Cash session variance facts
- Procurement PO/receipt facts
- Waste events
- Executive snapshot + insight engines
- PostgreSQL analytic functions (added RB-025)
