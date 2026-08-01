# 03 — EXISTING TEST ASSESSMENT

## Playwright (`tests/Browser`)

- **~162 tests / 36 specs** (chromium-desktop baseline previo: **161 PASS / 1 skip / 0 FAIL** @ VPS 2026-08-01)
- Config: `playwright.config.js` — `RESTBAR_BASE_URL`, workers=1, retries=1
- Auth: `helpers/auth.js` — MFA TOTP + `loginAsAdmin`
- POS: `helpers/pos.js`

### Cobertura por dominio

| Dominio | Specs | Suficiencia E2E Tab |
|---------|-------|---------------------|
| Smoke / Flags | smoke, feature-flags | Alta smoke |
| Auth | auth-extended | Media (falta multi-context formal) |
| Admin | admin | Media |
| Orders | orders-e2e, navigation, concurrency | Media–Alta |
| Kitchen | kitchen | Media |
| Cash | cash* | Media |
| Inventory | inventory* | Media |
| Procurement / FC | procurement, foodcost | Media |
| Analytics / DI / BR | analytics, DI, BR | Soft |
| Reports | reports | Media |
| Multitenant | multitenant* | Media (skip sin seed) |
| Security | security, a11y-idor | Media |
| Waiters/RBAC | waiters | Soft skip |
| Responsive / Perf / Regression | * | Media |

### Gaps vs mandato Tab Browser

| Gap | Acción |
|-----|--------|
| Contextos multi-usuario simultáneos formales | Crear `helpers/multi-context.js` + `E2ETab/*` |
| Flujo pedido multitab mesero+cocina+bar+cajero | Nuevo spec E2E-POS-02 |
| Tres tenants concurrentes | Nuevo E2E-MT-05 |
| Validación SQL en suite | Documentar queries en Evidence (manual/psql) |
| Firefox/WebKit | Extender projects tras chromium PASS |
| Seed Production | Procedimiento Dev/SQL en 05 |

## Unit (`RestBar.Tests`)

~98 tests: Cash SM, PO SM, FoodCost math, Forecast, BI math, Rules, Copilot, Analytics catalog, TenantScope, SignalR groups, Inventory recipe qty

## Prioridad de nuevas pruebas

1. `E2ETab/multitenant-contexts.spec.js`  
2. `E2ETab/pos-kds-multitab.spec.js`  
3. Extender hostile IDOR cash/payment  
4. Re-ejecutar suite completa chromium-desktop  
