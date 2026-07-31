# 06 — Backup

## Strategy

| Type | Tool | Frequency (recommended) |
|------|------|-------------------------|
| Logical full | `pg_dump -Fc` via `Com/backup-restbar-db.ps1` | Daily + pre-deploy |
| Volume | Docker volume `restbar_postgres_data` | Snapshot via host/VPS provider |
| Pre-cert dumps | Existing certification folders | Ad-hoc |

## Scripts

- `RestBar/Com/backup-restbar-db.ps1` — remote or `-LocalDocker`; uses `RESTBAR_SSH_PASSWORD` env (no hardcoded secret).
- `RestBar/Com/restore-restbar-from-dump.ps1` — requires `-ConfirmRestore YES`.

## Gaps

- No cron/systemd timer installed by default on VPS.
- In-app `BackupSettingsService` remains a stub (UI simulate).
- Offsite copy not automated.

**Overall backup:** **PASS WITH CONDITIONS**
