# 07 — Report Performance (RB-1002)

## Stack

- `SalesReportService` / `AdvancedReportsService` — heavy Include usage (pre-existing).
- `AnalyticsQueryService` — prefers `analytics.sp_*` where available.
- DI forecast — persists `di_forecast_runs`; soft-fail on persist.

## This cycle

- Indexes on `orders(CompanyId|BranchId, closed_at)` support closed-sales windows used by reports/analytics.
- No formula or KPI calculation changes.

## Observations

| ID | Item |
|----|------|
| O-RPT-01 | SalesReportService still materializes large graphs — candidate for Select/DTO projection in a follow-up (behavior-preserving). |
| O-RPT-02 | Export Excel/CSV memory bounded by ClosedXML — stream for &gt;50k rows later. |
| O-RPT-03 | Full EXPLAIN of every SP not executed in this window — require analytics schema privileges + sample. |

## Result parity

Report business rules **unchanged**. Only supporting indexes + EF read hygiene on shared order tables.
