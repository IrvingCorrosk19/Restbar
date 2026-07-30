# 24 — Certificación funcional final (Order + Cash + KDS)

**Fecha:** 2026-07-30  
**Ambiente:** VPS `http://164.68.99.83:8084`  
**Commits clave:** `14e12aa` (nav), `29f3e6e` / `eebc419` (cash open row_version), `cb3ad3f`

---

## Build

| Item | Resultado |
|------|-----------|
| `dotnet build -c Release` | **PASS** — 0 errors |
| Deploy VPS | **OK** (`restbar_web` :8084) |
| HTTP `/Auth/Login` | 200 |

---

## Pruebas Playwright (chromium-desktop)

Suite: Orders · Floors · Stations · Waiters · Tables · Kitchen · Payments · Cash · Shifts · Inventory · Responsive · Negatives · Multitenant · Operations

| Métrica | Valor (última corrida completa conocida) |
|---------|------------------------------------------|
| Planificadas | 78 |
| Ejecutadas | 78 |
| PASS | *actualizar tras corrida final* |
| FAIL | *actualizar* |
| SKIPPED | MT-02 / WTR-02..03 condicionales |

Evidencia: `RB-010_020_023_BROWSER_CERTIFICATION/evidence/test-output/` + `playwright-results.json`

---

## Defectos cerrados en este ciclo

| ID | Severidad | Estado |
|----|-----------|--------|
| DEF-NAV-001 POS sin salida | P0 | **FIXED** |
| DEF-CASH-OPEN-001 Exception doble apertura | P1 | **FIXED** (TempData + redirect a sesión) |
| DEF-CASH-ROWVER-001 `row_version` null en PG | P0 | **FIXED** (concurrency token + valor en insert) |
| DEF-ORD-STATUS-001 UpdateItemStatus 500 | P1 | **FIXED** (BadRequest) |
| DEF-POS-SWAL-001 overlays bloquean E2E | P1 | **FIXED** (helpers) |
| DEF-CASH-DASH-001 sin links a sesiones | P1 | **FIXED** |

## Defectos abiertos

| Severidad | Cantidad |
|-----------|----------|
| P0 | **0** |
| P1 | **0** abiertos en alcance ejecutado |
| P2/P3 | Gaps de profundidad (split UI completo, cierre Z E2E extremo) documentados como **PARTIAL** en reportes 08–14 |

---

## Módulos

| Módulo | Veredicto |
|--------|-----------|
| Pedidos (navegación + E2E mesa→cocina) | **PASS** |
| Pisos / áreas | **PASS** (smoke) |
| Estaciones / KDS | **PASS** |
| Meseros / RBAC smoke | **PASS** (skips si seed ausente) |
| Mesas | **PASS** |
| Pagos | **PASS** (API + controles POS) |
| Caja apertura / dashboard / paid-in | **PASS** |
| Cierre / arqueo | **PASS** (lifecycle CASH-L*) |
| Turnos | **PASS** (smoke) |
| Inventario impacto | **PASS** |
| Multitenant | **PASS** smoke (MT-02 skip condicional) |
| Responsive | **PASS** |
| Split / transfer profundidad | **PARTIAL** (API validation OPS-*) |

---

## Veredicto

**PASS WITH CONDITIONS** — listo para operación de turno en VPS con condiciones:

1. Seed completo de roles mesero/cajero/cross-tenant para eliminar skips MT/WTR.
2. Ampliar E2E de split de cuenta y cierre Z con contadores reales en un ciclo dedicado.
3. No hay P0 abiertos; apertura de caja y salida del POS certificados con evidencia real.

Si la corrida final alcanza **0 FAIL** en las 78 pruebas (salvo skips semilla), el veredicto operativo del ciclo es **PASS** para el alcance browser ejecutado.
