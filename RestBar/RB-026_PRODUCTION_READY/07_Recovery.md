# 07 — Recovery

## RTO / RPO targets (pilot)

| Objective | Target | Evidence |
|-----------|--------|----------|
| RPO | ≤ 24h with daily dump | Script exists; schedule ops-owned |
| RTO | ≤ 2h restore + verify + smoke | `restore-restbar-from-dump.ps1` + deploy |

## Disaster steps

1. Stop `restbar_web`.  
2. Restore dump with `-ConfirmRestore YES`.  
3. Start stack; hit `/health/ready`.  
4. Login smoke + Cash/Order smoke.  
5. Verify DataProtection volume intact (cookies); if keys lost, sessions invalidate (expected).

## Partial restore

Table-level restore possible via `pg_restore -t` (manual DBA); not scripted.

**Overall recovery:** **PASS WITH CONDITIONS**
