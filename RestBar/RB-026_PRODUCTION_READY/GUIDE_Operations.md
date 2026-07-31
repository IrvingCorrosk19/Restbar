# GUIDE — Operations

## Containers

```bash
docker ps --filter name=restbar
docker logs -f restbar_web --tail 200
docker exec -it restbar_postgres psql -U restbaruser -d RestBar
```

## Health

- Live: `/health/live`  
- Ready: `/health/ready`  
- Compose healthcheck uses live endpoint  

## Flags

Edit `appsettings.Production.json` or env `FeatureFlags__EnableCashModule=true` then recreate web container.

## Do not

- Expose Postgres port publicly  
- Run Seed in Production  
- Deploy without backup  
