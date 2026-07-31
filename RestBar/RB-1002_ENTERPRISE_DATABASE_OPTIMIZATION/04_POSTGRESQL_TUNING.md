# 04 — PostgreSQL Tuning (RB-1002)

## Observed (VPS container defaults)

- Workload: single-tenant-volume pilot (~80k audit rows, &lt;1k orders).
- Autovacuum: `audit_logs` dead_tup ≈ 0 after analyze; `orders` dead_tup 42 (healthy).
- No evidence of deadlock storms in deploy window.

## Changes this cycle

- `ANALYZE` on `audit_logs`, `orders`, `order_items`, `customers`, `products` after index create.
- Hot indexes (see 05).

## Tuning recommendations (ops — not applied without DBA window)

| Setting | Guidance | Risk |
|---------|----------|------|
| `shared_buffers` | 25% RAM of DB container | Restart required |
| `effective_cache_size` | ~50–75% host RAM | Planner only |
| `work_mem` | Raise carefully for sorts/hash joins on reports | Memory spikes |
| `max_connections` | Align with ASP.NET pool; prefer PgBouncer if &gt;100 app instances | Exhaustion |
| WAL / checkpoints | Monitor `pg_stat_bgwriter` under load | IO |

## Connection pooling

- Npgsql default pooling ON.
- App: `EnableRetryOnFailure(5)` — good for transient; does not replace pool sizing.

## Verdict on PG config

**No production GUCs changed** in RB-1002 — evidence-based index + EF first. GUC tuning requires dedicated load window and is tracked as Observation O-PG-01.
