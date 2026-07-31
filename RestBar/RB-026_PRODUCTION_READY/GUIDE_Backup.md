# GUIDE — Backup

```powershell
$env:RESTBAR_SSH_PASSWORD = "<ssh>"
# optional: $env:RESTBAR_SSH_HOSTKEY = "ssh-ed25519 SHA256:..."
.\RestBar\Com\backup-restbar-db.ps1
# or local:
.\RestBar\Com\backup-restbar-db.ps1 -LocalDocker
```

Schedule with Windows Task Scheduler or cron calling this script; copy dumps off-box.

Retain ≥ 7 daily + 4 weekly.
