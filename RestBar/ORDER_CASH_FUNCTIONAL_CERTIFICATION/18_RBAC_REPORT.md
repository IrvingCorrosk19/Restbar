# 18 — Informe RBAC / Autorización

| Campo | Valor |
|-------|-------|
| **Fecha** | 2026-07-30 |
| **Entorno** | VPS `http://164.68.99.83:8084` |
| **Specs** | `Waiters/waiters.spec.js`, `Security/security.spec.js` |

## Waiters / roles smoke

| ID | Descripción | Estado | Notas |
|----|-------------|--------|-------|
| WTR-01 | admin can open POS | **PENDING_FULL_SUITE** | |
| WTR-02 | mesero login or skip | **SKIP condicional** | Seed mesero |
| WTR-03 | cajero login cash access or skip | **SKIP condicional** | Seed cajero |
| WTR-04 | chef KDS access or skip | **SKIP condicional** | Seed chef |
| WTR-05 | logout clears order access | **PENDING_FULL_SUITE** | |

## Security gates

| ID | Descripción | Estado | Notas |
|----|-------------|--------|-------|
| SEC-01 | Anonymous CashSession → login | **PENDING_FULL_SUITE** | |
| SEC-02 | Anonymous Supplier → login | **PENDING_FULL_SUITE** | |
| SEC-03 | Anonymous FoodCost → login | **PENDING_FULL_SUITE** | |
| SEC-04 | Anonymous paid-out API not 500 | **PENDING_FULL_SUITE** | |
| SEC-05 | Admin can open Cash after login | **PENDING_FULL_SUITE** | |

## Limitaciones

Matriz RBAC completa (permiso × acción × módulo) **no implementada** en browser suite.

## Veredicto

**PARTIAL** — gates anónimos y smoke por rol; matriz RBAC **PENDING_FULL_SUITE** + seed roles.
