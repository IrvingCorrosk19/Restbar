# 13 — Informe Cambio / Movimientos de efectivo

| Campo | Valor |
|-------|-------|
| **Fecha** | 2026-07-30 |
| **Entorno** | VPS `http://164.68.99.83:8084` |
| **Specs** | `Cash/cash.spec.js`, `Cash/cash-extended.spec.js`, `Security/security.spec.js` |

## Cobertura Playwright

| ID | Descripción | Estado | Notas |
|----|-------------|--------|-------|
| CASH-05 | Paid-in API requires session (negative) | **PARTIAL** | Sin sesión válida → no 500 |
| CASH-X05 | paid-in negative still not 500 | **PARTIAL** | Guid vacío |
| SEC-04 | Anonymous paid-out API not 500 | **PENDING_FULL_SUITE** | Auth gate API |

## Gap declarado

**No existe** spec browser para:
- Paid-in / paid-out con sesión activa y saldo actualizado
- Cambio al cliente en flujo de pago POS
- Conciliación de movimientos en UI

## Veredicto

**PARTIAL (API-smoke only)** — endpoints no crashean en negativos; flujos operativos de cambio **NOT_COVERED**.
