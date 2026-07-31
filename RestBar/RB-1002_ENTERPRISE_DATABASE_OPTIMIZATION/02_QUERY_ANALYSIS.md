# 02 — Query Analysis (RB-1002)

## Anti-patterns found

| ID | Pattern | Location | Severity | Disposition |
|----|---------|----------|----------|-------------|
| Q1 | Seq Scan on `orders.status` kitchen filter | KitchenService | P0@scale | **Fixed** index `IX_orders_status_opened` |
| Q2 | OrderBy `audit_logs.timestamp` without index | AuditLogService | P0 | **Fixed** `IX_audit_logs_timestamp` + company composite |
| Q3 | Include + Select redundancy | KitchenService | P1 | **Removed** Includes; AsNoTracking |
| Q4 | Client-side `.Where(vm => vm.Items.Any())` after Select | KitchenService station | P1 | **Pushed** station/item filter into SQL `Where` |
| Q5 | Cartesian risk Include OrderItems+Product | OrderService.GetAll | P1 | **AsSplitQuery** + AsNoTracking |
| Q6 | Unbounded audit list | AuditLogService.GetAll/GetByCompany | P1 | Documented; pagination deferred (contract) |
| Q7 | Payment Include without NoTracking | PaymentService | P2 | **AsNoTracking** + SplitQuery |
| Q8 | Customer reads tracked | CustomerService | P2 | **AsNoTracking** on reads; tracking preserved for loyalty update |

## EXPLAIN — Kitchen status filter

### Before (Seq Scan)

```
Seq Scan on orders  (actual rows=12)
  Filter: status IN (SentToKitchen, Preparing)
  Rows Removed by Filter: 869
  Buffers: shared hit=27
Execution Time: ~0.36 ms
```

### After (Bitmap Index Scan)

```
Bitmap Index Scan on ix_orders_status_opened
Bitmap Heap Scan on orders (rows=12)
  Buffers: shared hit=4–7 (+ cold read on first hit)
Execution Time: ~0.17–0.53 ms (plan cost lower; scales with volume)
```

## EXPLAIN — Audit recent rows

### After

```
Index Scan using ix_audit_logs_timestamp
  Limit 100 — Execution Time ~0.25 ms
```

## Residual risks

- `GetAllAsync` on Orders/Payments/Audit still loads full result sets — OK for pilot volume; **must paginate** before 10k+ concurrent / multi-year audit retention.
- Kitchen board still not CompanyId-scoped in SQL (pre-existing behavior; MT filter may live in controller/hub — do not silently change without product sign-off).
