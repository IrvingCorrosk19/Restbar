# 10 — API Catalog

Base: `/api/analytics` Auth: AnalyticsView

- GET executive-summary, sales/trend, sales/products, profitability/products, inventory/health, purchases/suppliers, cash/summary, operations/kitchen, live, decisions, kpis, reports
- POST export (AnalyticsExport)

Tenant always from claims via AnalyticsScopeService.
