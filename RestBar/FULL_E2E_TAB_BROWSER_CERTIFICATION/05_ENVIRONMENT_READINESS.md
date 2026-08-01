# 05 — ENVIRONMENT READINESS

## Target

| Item | Valor |
|------|-------|
| URL | http://164.68.99.83:8084 |
| Health | GET `/health` → Healthy |
| Compose | restbar_web + restbar_postgres |
| FeatureFlags Prod | Cash/Purchasing/FoodCost/CC/DI/BR **true**; Copilot **false**; SeedEndpoints **false** |

## Checklist pre-ejecución

| Check | Estado |
|-------|--------|
| Health Healthy | PENDING VERIFY |
| Admin login + MFA | PENDING VERIFY |
| Costa/Norte/Sur admins existen | PENDING VERIFY |
| Playwright deps instalados | READY (tests/Browser) |
| Evidence folder | READY |
| Chromium browsers | READY via npx playwright |

## Variables

```text
RESTBAR_BASE_URL=http://164.68.99.83:8084
RESTBAR_ADMIN_EMAIL=admin@restbar.com
RESTBAR_ADMIN_PASSWORD=123456
RESTBAR_MFA_SECRET=JBSWY3DPEHPK3PXP
```

## Comando regresión

```powershell
cd RestBar/tests/Browser
$env:RESTBAR_BASE_URL="http://164.68.99.83:8084"
$env:RESTBAR_MFA_SECRET="JBSWY3DPEHPK3PXP"
npx playwright test --project=chromium-desktop
npx playwright test E2ETab --project=chromium-desktop
```

## Riesgos

- Seed HTTP bloqueado en Production  
- Latencia VPS → timeouts 60s (config actual)  
- MFA clock skew → ventana Totp ±1  
