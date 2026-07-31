# GUIDE — Installation (clean server)

## Prerequisites

- Ubuntu 22.04+ / Docker Engine + Compose plugin  
- Ports: **8084** (app), Postgres internal only  
- DNS + nginx if public HTTPS  

## Steps

```bash
mkdir -p /opt/apps/restbar && cd /opt/apps/restbar
git clone <repo-url> .
cp .env.example .env
# edit POSTGRES_PASSWORD
docker compose up -d --build
curl -fsS http://127.0.0.1:8084/health/ready
curl -fsS http://127.0.0.1:8084/health/live
```

Open `http://SERVER:8084/Auth/Login` — create/bootstrap admin via Dev Seed **only on Development**, or provision SQL user.

Wire nginx TLS using `RestBar/Com/restbar/nginx-*.conf` patterns.

## Rollback install

```bash
docker compose down
# restore volume from snapshot or dump
```
