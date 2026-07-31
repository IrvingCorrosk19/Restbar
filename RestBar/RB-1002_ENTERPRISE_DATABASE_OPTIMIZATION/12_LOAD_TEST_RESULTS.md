# 12 — Load Test Results (RB-1002)

## Executed

| Test | Result |
|------|--------|
| Unit `RestBar.Tests` Release | **98 PASS** |
| Playwright critical (prior session) Auth/DI/BR/Smoke/MT | **22 PASS** / 1 skip |
| PostgreSQL EXPLAIN ANALYZE kitchen + audit | Plans improved (see 02) |
| Synthetic 100 / 500 / 1k / 5k / 10k users | **NOT RUN** |

## Why load levels deferred

- No dedicated load lab / k6 suite wired to VPS in this program window.
- Shared VPS hosts multiple apps — 10k VU risk to co-tenants.
- Current data volume (&lt;1k orders) cannot extrapolate P99 under 10k VU honestly.

## Proxy metrics (DB)

| Query | Before plan | After plan | Exec (sample) |
|-------|-------------|------------|---------------|
| Kitchen status filter | Seq Scan, hit≈27 | Bitmap Index `ix_orders_status_opened`, hit≈4–7 | ~0.2–0.5 ms |
| Audit ORDER BY timestamp LIMIT 100 | (would Seq+Sort at scale) | Index Scan `ix_audit_logs_timestamp` | ~0.25 ms |

## Observation O-LOAD-01

Publish **WORLD CLASS** load certification only after k6/NBomber against staging clone with production-like data.
