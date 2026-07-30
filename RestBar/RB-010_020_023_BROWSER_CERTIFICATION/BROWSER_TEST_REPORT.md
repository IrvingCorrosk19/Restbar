# BROWSER_TEST_REPORT — RB-010 / RB-020 / RB-023

**Fecha ejecución:** 2026-07-29 (hora local) / 2026-07-30 UTC  
**Ambiente:** `http://localhost:5001` · `ASPNETCORE_ENVIRONMENT=Development`  
**Herramienta:** Playwright 1.49 · Chromium  
**Credencial:** `admin@restbar.com` / `123456`

## Resultado agregado (ejecución real)

| Project | Passed | Failed | Skipped | Exit |
|---------|--------|--------|---------|------|
| chromium-desktop | **40** | 0 | 2 | 0 |
| chromium-tablet (834×1194) | **40** | 0 | 2 | 0 |
| chromium-mobile (412×915) | **40** | 0 | 2 | 0 |
| **TOTAL** | **120** | **0** | **6** | **PASS** |

Skips esperados: sin cajas registradoras sembradas / sin link “Ver costo” / `admin@costa.restbar.com` no sembrado.

## Suite

`RestBar/tests/Browser/` — Smoke, Cash, Procurement, FoodCost, Regression, Security, Multitenant, Performance.

## Logs

- `playwright-run-desktop-final.log`
- `playwright-run-responsive2.log`
- `playwright-results.json` / `playwright-report/`

## Defectos descubiertos y corregidos durante la corrida

Ver `BUG_REPORT.md`.
