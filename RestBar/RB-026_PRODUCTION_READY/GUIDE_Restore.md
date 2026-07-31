# GUIDE — Restore

```powershell
$env:RESTBAR_SSH_PASSWORD = "<ssh>"
.\RestBar\Com\restore-restbar-from-dump.ps1 -DumpPath .\backups\RestBar_backup_YYYYMMDD.dump -ConfirmRestore YES
```

Then:

```bash
curl -fsS http://127.0.0.1:8084/health/ready
# login smoke
```

**Warning:** `--clean --if-exists` replaces objects in RestBar DB only (not other VPS apps).
