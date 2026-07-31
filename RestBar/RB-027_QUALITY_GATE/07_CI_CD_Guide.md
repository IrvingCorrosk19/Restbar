# 07 — CI/CD Guide

## Pipeline actual (RB-027)

Archivo: `.github/workflows/restbar-ci.yml`

```
push/PR → main|master
  ├─ G1 Build
  ├─ G2 Unit tests + coverage artifact
  ├─ G3 Security advisory + policy assets
  ├─ G4 Browser smoke (si vars.RESTBAR_BASE_URL)
  └─ Quality Gate (agregado; requiere G1–G3 success)
```

## Configuración obligatoria en GitHub

1. **Settings → Branches → Branch protection** on `main`:
   - Require status checks: **`Quality Gate`**
   - Require branches up to date
   - No force push
2. **Settings → Variables:** `RESTBAR_BASE_URL` = staging/VPS URL (ej. `http://164.68.99.83:8084`)
3. **Settings → Secrets:** `RESTBAR_ADMIN_EMAIL`, `RESTBAR_ADMIN_PASSWORD` (smoke account no-prod-break)

Sin (2), G4 no corre automáticamente — **condición explícita** del veredicto.

## Local

```powershell
pwsh RestBar/Com/quality/run-quality-gates.ps1 -BaseUrl http://localhost:5001
```

## Deploy

- Script: `RestBar/Com/deploy-restbar.ps1` (VPS Docker, no toca otras apps).
- **Regla:** solo desplegar commit con CI verde.
- Health post-deploy: `/health/live`, `/health/ready`.
- DB role VPS: **`restbaruser`** (nunca `postgres`). Helper: `Com/psql-restbar-vps.ps1`.

## Reportes

| Artefacto | Origen |
|-----------|--------|
| coverage.cobertura.xml | G2 artifact `unit-coverage` |
| vuln-report.txt | G3 artifact |
| Playwright HTML | local / `RB-010_020_023_BROWSER_CERTIFICATION` |

## Qué falta para “no desplegar inseguro” al 100%

| Capacidad | Estado |
|-----------|--------|
| Bloquear merge si unit falla | ✅ vía Quality Gate |
| Bloquear merge si browser falla | ⚠️ solo con variable URL |
| Bloquear deploy automático | ⚠️ deploy es manual; disciplina operativa |
| SAST CodeQL | ❌ backlog |
| Container image scan | ❌ backlog |
