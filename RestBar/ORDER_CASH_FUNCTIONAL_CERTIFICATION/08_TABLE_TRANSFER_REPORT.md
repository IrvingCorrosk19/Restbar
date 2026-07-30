# 08 — Informe Transferencia de Mesa

| Campo | Valor |
|-------|-------|
| **Fecha** | 2026-07-30 |
| **Entorno** | VPS `http://164.68.99.83:8084` |
| **Specs** | `Orders/orders-e2e.spec.js` (ORD-E2E-05), `Tables/tables.spec.js` |

## Cobertura real en Playwright

| ID | Spec | Descripción | Estado | Notas |
|----|------|-------------|--------|-------|
| ORD-E2E-05 | orders-e2e | MoveToTable API validation | **PARTIAL** | POST con Guids inválidos → no 500; **sin flujo UI** |
| TBL-01 | tables | Tables management page | **PENDING_FULL_SUITE** | CRUD mesas |
| TBL-02 | tables | POS lists multiple tables | **PENDING_FULL_SUITE** | |
| TBL-03 | tables | select table enables order surface | **PENDING_FULL_SUITE** | |
| TBL-04 | tables | GetActiveTables no 500 | **PENDING_FULL_SUITE** | |

## Gap declarado

**No existe** spec de browser que ejecute transferencia mesa→mesa con pedido activo, items en cocina y verificación de estado post-move.

## Veredicto

**PARTIAL (API-smoke only)** — integridad endpoint validada; E2E transferencia **NOT_COVERED**.
