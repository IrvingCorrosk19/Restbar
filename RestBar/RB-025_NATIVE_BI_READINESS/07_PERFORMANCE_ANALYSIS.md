# 07 — Performance Analysis

## Target

Interactive native BI endpoints ideally **&lt; 2s** at branch scope for typical 30-day windows.

## What helps today (evidence)

| Mechanism | Evidence |
|-----------|----------|
| Branch+date indexes on orders/payments | `IX_orders_branch_*`, `IX_payments_branch_paid_at` |
| New closed_at index | `IX_orders_branch_closed` |
| Inventory company/product/branch date indexes | `IX_inv_mov_*` |
| Set-based PostgreSQL functions | `Sql/Bi/01_native_bi_functions.sql` |
| Command Center in-memory cache | `ExecutiveCommandCenterService` ConcurrentDictionary |
| Snapshot tables | `executive_snapshots`, `food_cost_snapshots` |

## Risks

| Risk | Evidence | Mitigation |
|------|----------|------------|
| Legacy SalesReport ignores BranchId in some metrics | `SalesReportService.ApplyFilters` | Prefer BiNative SPs / fix filters |
| AdvancedReports loads large graphs in memory | Operational analysis ToList | Prefer SPs; paginate |
| Dual LINQ N+1 patterns in older reports | multiple Include | SP layer |
| Company-wide branch comparison scans all branches | `sp_branch_comparison` | OK for small tenant; add date partition later |
| Materialized views | None yet | Only add if EXPLAIN proves need |

## Materialized views

**Not created.** Justification: current volumes + indexes + snapshots are sufficient for readiness; MVs add refresh ops without proven bottleneck.

## Guidance for &lt;2s

1. Always pass company+branch+closed_at range.
2. Use `sp_*` for aggregates; avoid loading full order graphs.
3. Keep Command Center cache TTL behavior.
4. Paginate top-N (SPs already `LIMIT`).
5. Measure with `EXPLAIN (ANALYZE, BUFFERS)` on VPS after migrate.

## AsNoTracking / pagination

- New BiNative service uses raw SQL (no change tracker load of entities).
- Hub tables are client-side filtered for small top-N sets.
- Large exports should stream (future) — not part of readiness gate.
