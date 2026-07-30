# 15 — Informe Turnos / Usuarios operativos

| Campo | Valor |
|-------|-------|
| **Fecha** | 2026-07-30 |
| **Entorno** | VPS `http://164.68.99.83:8084` |
| **Spec** | `tests/Browser/Shifts/shifts.spec.js` |

## Resultados

| ID | Descripción | Estado | Evidencia / notas |
|----|-------------|--------|-------------------|
| SHF-01 | Users page loads | **PENDING_FULL_SUITE** | `/User` |
| SHF-02 | User assignments page | **PENDING_FULL_SUITE** | `/UserAssignment` |
| SHF-03 | Cash session survives navigation | **PENDING_FULL_SUITE** | Dashboard → POS → Dashboard sin Exception |

## Alcance NO cubierto

| Flujo | Estado |
|-------|--------|
| Apertura/cierre turno mesero | **NOT_COVERED** |
| Handoff entre cajeros | **NOT_COVERED** |
| Restricción operaciones fuera de turno | **NOT_COVERED** |

## Veredicto

**PENDING_FULL_SUITE** — smoke de páginas; lógica de turnos E2E **NOT_COVERED**.
