# 11 — Informe Cancelaciones

| Campo | Valor |
|-------|-------|
| **Fecha** | 2026-07-30 |
| **Entorno** | VPS `http://164.68.99.83:8084` |
| **Specs** | `Regression/order-cash-negatives.spec.js`, `Stations/stations.spec.js` |

## Cobertura Playwright

| ID | Spec | Descripción | Estado | Notas |
|----|------|-------------|--------|-------|
| NEG-03 | order-cash-negatives | cancel item invalid ids | **PARTIAL** | API UpdateItemStatus `Cancelled` + Guids vacíos → no 500 |
| STN-05 | stations | UpdateItemStatus rejects empty payload | **PASS** | BadRequest post-fix `29f3e6e` |

## Gap declarado

**No existe** spec que cancele ítem real desde POS/KDS y verifique inventario, totales y estado de mesa.

## Veredicto

**PARTIAL (API-smoke only)** — rechazo seguro de cancel inválida; cancelación operativa E2E **NOT_COVERED**.
