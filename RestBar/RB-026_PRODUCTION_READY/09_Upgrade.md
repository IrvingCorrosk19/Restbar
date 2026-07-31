# 09 — Upgrade

## Safe upgrade procedure

1. `Com/backup-restbar-db.ps1` (retain dump).  
2. `git fetch && git checkout <tag/commit>`.  
3. `docker compose up -d --build`.  
4. App runs `Database.Migrate()` on boot — watch logs.  
5. Smoke: `/health/ready`, login, POS, Cash.  
6. If fail: rollback image/tag + restore dump if migration destructive (avoid destructive migrations).

## Feature flags

Use `FeatureFlags` to disable Cash/Purchasing/FoodCost/CommandCenter without redeploy of code paths (`ModuleDisabled`).

## Classification

**PASS WITH CONDITIONS** — EF migrate-on-startup works; no blue/green; no automated migration dry-run gate.
