# 04 — Informe Pisos / Áreas

| Campo | Valor |
|-------|-------|
| **Fecha** | 2026-07-30 |
| **Entorno** | VPS `http://164.68.99.83:8084` |
| **Spec** | `tests/Browser/Floors/floors.spec.js` |
| **Commits** | `14e12aa`, `33e47e2` |

## Resultados

| ID | Descripción | Estado | Evidencia / notas |
|----|-------------|--------|-------------------|
| FLR-01 | Areas index loads | **PENDING_FULL_SUITE** | `/Area` sin 500 |
| FLR-02 | POS tables expose area metadata | **PENDING_FULL_SUITE** | `.table-card[data-table-area]` |
| FLR-03 | filter Todas visible on POS | **PENDING_FULL_SUITE** | Botón "Todas" |
| FLR-04 | exit POS after browsing tables preserves Home | **PENDING_FULL_SUITE** | Depende fix nav (`order-nav-back`) |

## Contexto

- Fix P0 navegación (`14e12aa`) beneficia FLR-04 (salida POS → Home).
- Run previo global: 79 pass / 8 fail / 1 skip; fallos corregidos en ciclo posterior.
- Suite Floors no incluida en retest focalizado 2026-07-30.

## Veredicto

**PENDING_FULL_SUITE** — specs existen; ejecución completa pendiente de run padre.
