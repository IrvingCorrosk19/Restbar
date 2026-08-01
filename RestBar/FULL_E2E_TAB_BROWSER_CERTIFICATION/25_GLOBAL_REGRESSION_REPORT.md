# 25 — GLOBAL REGRESSION REPORT (Tab Browser)

**Dominio:** Full chromium-desktop Playwright suite  
**Pack fecha:** 2026-08-01  
**Build:** Release deploy `871abc7`

| ID | Escenario | Estado | Notas |
|----|-----------|--------|-------|
| E2E-REG-01 | Full chromium-desktop regression | **IN PROGRESS** | Re-run global en curso (2026-08-01) |
| Prior baseline chromium-desktop | 161 PASS / 1 skip / 0 FAIL | Referencia (2026-08-01) | Baseline previo al cierre de este pack |
| E2ETab suite | 5/5 PASS | **PASS** | Tras rate-limit fix (BUG-E2E-001) |
| Unit RestBar.Tests | ~98 pass | Referencia histórica | Cash SM, PO, FC, Forecast, BI, TenantScope, SignalR |

## Log / evidencia

- E2ETab: `e2e-tab-retest.log` → 5 passed  
- Global: `global-regression.log` (IN PROGRESS)  
- No declarar PASS global hasta completar re-run sin FAIL

## Gaps vs mandato

- Firefox / WebKit no parte del cert primario aún  
- Global regression debe cerrar sin FAIL para elevar veredicto REGRESSION

**Veredicto dominio Regression:** IN PROGRESS (baseline histórico limpio; re-run no cerrado).
