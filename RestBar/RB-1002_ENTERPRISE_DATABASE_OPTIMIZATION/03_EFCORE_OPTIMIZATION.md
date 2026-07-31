# 03 — EF Core Optimization (RB-1002)

## Applied

| Change | Files | Benefit | Business impact |
|--------|-------|---------|-----------------|
| `AsNoTracking()` on read boards | KitchenService, OrderService GetAll/GetById, PaymentService Get*, CustomerService reads, AuditLogService reads | Lower change-tracker CPU/RAM | None (reads) |
| `AsSplitQuery()` on multi-Include | OrderService.GetAll, Payment Get*, Audit lists, Customer with orders | Avoid cartesian explosion | Same graph shape |
| Select-only kitchen DTO | KitchenService | Less materialization | Same ViewModels |
| Station filter in SQL | KitchenService.GetPendingOrdersByStationTypeAsync | Fewer orders pulled | Same station filter semantics |
| Loyalty update tracking fix | CustomerService.UpdateLoyaltyPointsAsync | Correct SaveChanges after NoTracking GetById | Preserves write behavior |

## Not applied (intentionally)

| Technique | Why deferred |
|-----------|--------------|
| Global `QueryTrackingBehavior.NoTracking` | Breaks write paths relying on tracking |
| Compiled models / compiled queries | High churn cost; volume still small |
| ExecuteUpdate/ExecuteDelete bulk | Would require rewrite of domain flows |
| AutoMapper ProjectTo | Not in stack; Select already used where hot |

## DbContext lifetime

- Scoped per request (correct).
- Retry on failure + 30s command timeout already in `Program.cs`.
- `PendingModelChangesWarning` ignored for SQL-only enterprise migrations (ops necessity).

## Metrics (logical)

| Metric | Before | After |
|--------|--------|-------|
| Tracker entries on Kitchen poll | Full Order+Items+Product graph | 0 (NoTracking + project) |
| Kitchen SQL shape | Include+Select | Select only |
| Split queries Order GetAll | 1 fat join | Split (multiple round-trips, smaller rows) |
