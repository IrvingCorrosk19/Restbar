# 06 — Informe Meseros / RBAC smoke

| Campo | Valor |
|-------|-------|
| **Fecha** | 2026-07-30 |
| **Entorno** | VPS `http://164.68.99.83:8084` |
| **Spec** | `tests/Browser/Waiters/waiters.spec.js` |

## Resultados

| ID | Descripción | Estado | Evidencia / notas |
|----|-------------|--------|-------------------|
| WTR-01 | admin can open POS | **PENDING_FULL_SUITE** | POS chrome visible |
| WTR-02 | mesero login or skip | **SKIP condicional** | Skip si `mesero@restbar.com` no sembrado |
| WTR-03 | cajero login cash access or skip | **SKIP condicional** | Skip si `cajero@restbar.com` no sembrado |
| WTR-04 | chef KDS access or skip | **SKIP condicional** | Skip si `chef@restbar.com` no sembrado |
| WTR-05 | logout clears order access | **PENDING_FULL_SUITE** | Redirect a `/Auth/Login` |

## Limitaciones

- Suite es **smoke RBAC**, no matriz completa de permisos por acción.
- Roles no-admin dependen de seed en VPS.

## Veredicto

**PARTIAL** — admin path esperado PASS; roles operativos **SKIP** hasta seed dedicado.
