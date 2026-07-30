# 22 — Informe Regresión

| Campo | Valor |
|-------|-------|
| **Fecha** | 2026-07-30 |
| **Entorno** | VPS `http://164.68.99.83:8084` |
| **Specs** | `Regression/regression.spec.js`, `Regression/order-cash-negatives.spec.js`, `Smoke/smoke.spec.js` |

## Core POS regression

| ID | Descripción | Estado | Evidencia / notas |
|----|-------------|--------|-------------------|
| REG-01 | Order index | **PENDING_FULL_SUITE** | `reg-01-orders.png` |
| REG-02 | Kitchen station orders | **PENDING_FULL_SUITE** | `reg-02-kitchen.png` |
| REG-03 | Product index | **PENDING_FULL_SUITE** | |
| REG-04 | Inventory index | **PENDING_FULL_SUITE** | |
| REG-05 | Command Center | **PENDING_FULL_SUITE** | `reg-05-cc.png` |
| REG-06 | Logout path exists | **PENDING_FULL_SUITE** | |

## Negatives / integridad

| ID | Descripción | Estado | Notas |
|----|-------------|--------|-------|
| NEG-01 | SendToKitchen empty items fails softly | **PENDING_FULL_SUITE** | API |
| NEG-02 | foreign order payment summary | **PENDING_FULL_SUITE** | API |
| NEG-03 | cancel item invalid ids | **PARTIAL** | API UpdateItemStatus |
| NEG-04 | POS with product then home no crash | **PENDING_FULL_SUITE** | Nav fix relacionado |

## Smoke auth

| ID | Descripción | Estado |
|----|-------------|--------|
| SMK-01 | Login admin succeeds | **PENDING_FULL_SUITE** |
| SMK-02 | Unauthenticated protected route redirects | **PENDING_FULL_SUITE** |
| SMK-03 | Orders index loads after login | **PENDING_FULL_SUITE** |

## Historial run

| Métrica | Valor |
|---------|-------|
| Run previo completo | 79 pass / 8 fail / 1 skip |
| Post-fix retest focalizado | INV-ORD-01, KDS-03, ORD-E2E-01, PAY-01..04 **PASS** |

## Veredicto

**PENDING_FULL_SUITE** — regresión core pendiente re-run completo; fallos previos abordados en fixes documentados.
