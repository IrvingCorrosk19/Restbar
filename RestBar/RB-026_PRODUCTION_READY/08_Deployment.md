# 08 — Deployment

## Supported path

1. Provision VPS + Docker.  
2. Clone repo to `/opt/apps/restbar`.  
3. Copy `.env.example` → `.env` (strong passwords).  
4. `docker compose up -d --build` (root compose).  
5. Verify `curl http://127.0.0.1:8084/health/ready`.  
6. Configure nginx TLS (`Com/restbar/nginx-*.conf`).  
7. Set `Security:RequireSecureCookies=true` when HTTPS-only.

Script: `Com/deploy-restbar.ps1` (legacy; migrate secrets to env).

## CI

GitHub Actions `.github/workflows/restbar-ci.yml` — build + unit tests on push/PR.

## Classification

| Item | Status |
|------|--------|
| Dockerized install | **PASS** |
| Isolated network/ports | **PASS** |
| Automated CD to VPS | **FAIL** (manual) |
| Secrets management | **PASS WITH CONDITIONS** |

**Overall deployment:** **PASS WITH CONDITIONS**
