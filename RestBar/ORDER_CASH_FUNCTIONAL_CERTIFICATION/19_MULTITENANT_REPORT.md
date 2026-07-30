# 19 — Informe Multitenant / Aislamiento

| Campo | Valor |
|-------|-------|
| **Fecha** | 2026-07-30 |
| **Entorno** | VPS `http://164.68.99.83:8084` |
| **Spec** | `tests/Browser/Multitenant/multitenant.spec.js` |

## Resultados

| ID | Descripción | Estado | Evidencia / notas |
|----|-------------|--------|-------------------|
| MT-01 | Login binds company claim (admin session) | **PENDING_FULL_SUITE** | Screenshot `mt-01-tenant.png` |
| MT-02 | Cross-company admin login if seeded | **SKIP frecuente** | `admin@costa.restbar.com` no sembrado |

## Limitaciones

- MT-02 es el único test cross-tenant; skip habitual en VPS actual.
- No hay specs de fuga de datos (orden/mesa/inventario tenant A visible en tenant B).

## Veredicto

**PARTIAL** — tenant primario smoke; aislamiento cross-company **SKIP / NOT_VERIFIED**.
