# GUIDE — Upgrade & Rollback

## Upgrade

Backup → pull → `docker compose up -d --build` → health → smoke.

## Rollback

```bash
git checkout <previous-sha>
docker compose up -d --build
# if schema forward-only broke: restore dump from pre-upgrade backup
```

Never edit production data to “make tests pass”.
