# 01 — Database Audit (RB-1002)

**Date:** 2026-07-31  
**Environment:** VPS PostgreSQL (`restbar_postgres`) + EF Core 9 / ASP.NET Core 8  
**Scope:** Discovery of entities, DbSets, indexes, SQL assets, hot services

## Inventory

| Asset | Count / Note |
|-------|----------------|
| DbSets (RestBarContext + partials) | ~42+ entity sets (Orders, OrderItems, Payments, Cash*, BI*, DI*, BR*, AuditLogs, …) |
| Public indexes (pre RB-1002) | **246** |
| Largest table | `audit_logs` **~81,458** live rows |
| Operational volume | `order_items` 1,601 · `orders` 881 · `payments` 323 · `products` 125 |
| SQL scripts | `Sql/Bi/*`, `Sql/DecisionIntelligence/*`, `Sql/BusinessRules/*`, `Sql/Performance/*` |
| Hosted / cache | `IMemoryCache` (auth reset tokens only); no Redis; no ResponseCaching middleware |

## Dependency map (hot paths)

```
POS / Kitchen  → OrderService, KitchenService, OrderHubService → orders, order_items, tables, stations, products
Payments       → PaymentService (+ CashPaymentHook) → payments, orders, cash_sessions
Cash           → CashSessionService / Movements → cash_*
Reports        → SalesReportService, AdvancedReportsService, AnalyticsQueryService → orders/payments + analytics.sp_*
Audit UI       → AuditLogService → audit_logs (+ User/Company/Branch Includes)
DI / BR        → DecisionIntelligenceService, BusinessRulesEngine → di_*, br_*, BI SPs
```

## Findings (audit)

1. **Indexes exist** for BranchId/CompanyId on core ops tables; kitchen status path was **Seq Scan**.
2. **audit_logs** lacked `(CompanyId, timestamp)` / `timestamp DESC` — list APIs order by Timestamp.
3. Many **read** paths tracked entities unnecessarily (`OrderService.GetAll`, `PaymentService.Get*`, `AuditLogService`, Kitchen projections with redundant Includes).
4. **Unbounded `ToListAsync`** on Audit GetByCompany / GetAll (functional risk at scale; not changed to avoid result-set contract change).
5. No distributed cache; SignalR tenant groups already scoped (RB-1001).

## Actions taken in this program

- Applied `Sql/Performance/01_rb1002_hot_indexes.sql` (+ EF migration `20260731120000_Rb1002PerformanceIndexes`).
- AsNoTracking / AsSplitQuery / Select push-down on Kitchen, Order GetAll/GetById, Payment Get*, Customer reads, AuditLog reads.
