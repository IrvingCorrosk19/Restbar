# 23 — Índice de evidencia de pruebas

| Campo | Valor |
|-------|-------|
| **Fecha** | 2026-07-30 |
| **Entorno** | VPS `http://164.68.99.83:8084` |
| **Commits** | `14e12aa`, `29f3e6e`, `33e47e2` |

## Artefactos Playwright

| Artefacto | Ruta |
|-----------|------|
| Config | `tests/Browser/playwright.config.js` |
| Helpers auth | `tests/Browser/helpers/auth.js` |
| Helpers POS | `tests/Browser/helpers/pos.js` |
| JSON resultados (retest focalizado) | `RB-010_020_023_BROWSER_CERTIFICATION/playwright-results.json` |
| Reporte HTML | `RB-010_020_023_BROWSER_CERTIFICATION/playwright-report/` |
| Output tests | `RB-010_020_023_BROWSER_CERTIFICATION/evidence/test-output/` |

## Screenshots por suite

| Archivo | Test / contexto |
|---------|-----------------|
| `evidence/smk-01-login.png` | Smoke login |
| `evidence/reg-01-orders.png` | REG-01 Order index |
| `evidence/reg-02-kitchen.png` | REG-02 Kitchen |
| `evidence/reg-05-cc.png` | REG-05 Command Center |
| `evidence/cash-01-dashboard.png` | CASH-01 Dashboard |
| `evidence/cash-02-open-wizard.png` | CASH-02 Open wizard |
| `evidence/cash-03-open-result.png` | CASH-03 Open session |
| `evidence/cash-04-registers.png` | CASH-04 Registers |
| `evidence/cash-07-responsive.png` | CASH-07 / RSP |
| `evidence/mt-01-tenant.png` | MT-01 Tenant |
| `evidence/mt-02-costa.png` | MT-02 (si no skip) |

## Tests verificados con evidencia JSON (2026-07-30 retest)

| ID | Status | Duración aprox. |
|----|--------|---------------|
| INV-ORD-01 | PASS | ~11s |
| KDS-03 | PASS | — |
| ORD-E2E-01 | PASS | — |
| PAY-01 | PASS | — |
| PAY-02 | PASS | — |
| PAY-03 | PASS | — |
| PAY-04 | PASS | — |

## Documentación certificación (este paquete)

| # | Archivo | Tema |
|---|---------|------|
| 01 | `01_NAVIGATION_ROOT_CAUSE.md` | Causa raíz nav P0 |
| 02 | `02_NAVIGATION_FIX.md` | Fix nav |
| 03 | `03_TEST_DATA_MATRIX.md` | Matriz datos |
| 04–20 | `04_*` … `20_*` | Informes por módulo |
| 21 | `21_BUG_REPORT.md` | Defectos |
| 22 | `22_REGRESSION_REPORT.md` | Regresión |
| 23 | `23_TEST_EVIDENCE_INDEX.md` | Este índice |
| 24 | `24_FINAL_FUNCTIONAL_CERTIFICATION.md` | Resumen final (padre) |

## Comando reproducción retest focalizado

```bash
cd tests/Browser
npx playwright test --project=chromium-desktop \
  Orders/orders-e2e.spec.js:6 \
  Kitchen/kitchen.spec.js:19 \
  Payments/payments.spec.js \
  Inventory/inventory-order-impact.spec.js:6
```

## Veredicto índice

Evidencia **parcial pero real** — retest focalizado documentado; suite completa pendiente run padre.
