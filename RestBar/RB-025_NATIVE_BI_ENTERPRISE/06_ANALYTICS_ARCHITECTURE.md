# 06 — Analytics Architecture

```
PostgreSQL (OLTP)
  → public.sp_* (RB-025 base)
  → analytics schema views + analytics.sp_*
  → AnalyticsQueryService (ADO + BiNative single path)
  → Analytics API / ExecutiveAnalytics MVC
  → HTML + Chart.js + CSV/XLSX/PDF(HTML fallback)
```

Single KPI path: executive totals reuse `IBiNativeAnalyticsService` / `sp_executive_dashboard` — no duplicate formulas.
