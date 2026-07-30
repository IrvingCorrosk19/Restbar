# 14 — Informe Cierre de Caja / Arqueo

| Campo | Valor |
|-------|-------|
| **Fecha** | 2026-07-30 |
| **Entorno** | VPS `http://164.68.99.83:8084` |
| **Specs** | `Cash/cash.spec.js` (parcial) |

## Cobertura Playwright

| ID | Descripción | Estado | Notas |
|----|-------------|--------|-------|
| CASH-06 | Verify chain endpoint no 500 | **PARTIAL** | GET verify con Guid vacío — integridad API |
| — | Close session / arqueo UI | **NOT_COVERED** | Sin test ID dedicado |
| — | Reporte Z / cierre turno caja | **NOT_COVERED** | |

## Gap declarado

Ningún spec ejecuta: sesión abierta → movimientos → cierre → arqueo → bloqueo de nuevas operaciones.

## Veredicto

**NOT_COVERED / PARTIAL** — cierre de caja **no certificado** en browser; solo smoke API verify.
