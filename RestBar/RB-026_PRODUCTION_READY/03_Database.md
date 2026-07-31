# 03 — Database

## Evidence

- 17 EF migrations; startup `Database.Migrate()`.
- Operational indexes: orders, order_items, payments, inventory, cash, procurement (migrations 20260729*, 20260730*).
- `EnableRetryOnFailure(5)` + `CommandTimeout(30)` added in RB-026.
- Postgres Docker healthcheck `pg_isready`.

## Gaps

| Item | Status |
|------|--------|
| Indexes for hot paths | **PASS WITH CONDITIONS** |
| FK / constraints | **PASS WITH CONDITIONS** (legacy mixed) |
| N+1 systematic audit | **PASS WITH CONDITIONS** (not fully instrumented) |
| Autovacuum / ANALYZE runbook | **PASS WITH CONDITIONS** (defaults; document ops) |
| Scheduled backup job | Scripts added; cron not installed on VPS by default | **PASS WITH CONDITIONS** |
| Load test 1M rows | Not run on prod | **NOT APPLICABLE** / deferred |

**Overall database:** **PASS WITH CONDITIONS**
