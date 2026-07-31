# 12 — Operations

See also guides in this folder:

- `GUIDE_Installation.md`
- `GUIDE_Operations.md`
- `GUIDE_Backup.md`
- `GUIDE_Restore.md`
- `GUIDE_Monitoring.md`
- `GUIDE_Security.md`
- `GUIDE_Troubleshooting.md`
- `GUIDE_Upgrade_Rollback.md`

## Day-2 checklist

| Task | Cadence |
|------|---------|
| `/health/ready` probe | 1 min (Docker/nginx) |
| `pg_dump` backup | Daily |
| Review container logs | Daily |
| Disk volume growth | Weekly |
| Dependency CVEs (MailKit noted) | Monthly |

**Overall operations:** **PASS WITH CONDITIONS**
