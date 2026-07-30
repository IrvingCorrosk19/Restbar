# 24 — Certificación funcional final (Order + Cash + KDS)

**Fecha:** 2026-07-30  
**Ambiente:** VPS `http://164.68.99.83:8084`  
**Commits:** `14e12aa` … `eebc419` (cash open) · local `e07bf33` (timeouts; push pendiente si red cae)

---

## Build

| Item | Resultado |
|------|-----------|
| `dotnet build -c Release` | **PASS** — 0 errors |
| Deploy VPS | **OK** hasta `eebc419` / `cb3ad3f` |
| Apertura caja (debug real) | **PASS** → `/CashSession/Detail/{id}` Sesión #1 Open |

---

## Pruebas Playwright (chromium-desktop)

| Métrica | Valor |
|---------|-------|
| Planificadas | 78 |
| Ejecutadas | 78 |
| **PASS** | **72** |
| **FAIL** | **4** (timeouts de red al VPS en retest: KDS-03, ORD-E2E-02, ORD-NAV-02, PAY-03) |
| **SKIPPED** | **2** (seed MT/WTR) |

**Cash lifecycle CASH-L01..L04:** **4/4 PASS**  
**OPS-01..03:** **3/3 PASS**  
**ORD-E2E-01 / ORD-E2E-03..05 / STN / FLR / INV / CASH-X\* / CASH-01..07:** **PASS** en la corrida estable

Evidencia: `RB-010_020_023_BROWSER_CERTIFICATION/evidence/test-output/`

---

## Defectos

| Severidad | Abiertos |
|-----------|----------|
| P0 | **0** |
| P1 | **0** |
| P2 | Seed / cobertura split UI / timeouts infra |

---

## Módulos

| Módulo | Veredicto |
|--------|-----------|
| Pedidos navegación | **PASS** |
| Pedidos E2E mesa→producto→cocina | **PASS** (ORD-E2E-01; 02 flaky por red) |
| Pisos / estaciones / mesas | **PASS** |
| KDS | **PASS** (boards + STN; send flaky por red) |
| Pagos | **PASS** (API + PAY-01/02/04) |
| Caja apertura / dashboard / arqueo / paid-in | **PASS** |
| Cancel / split / transfer API | **PASS** (OPS) |
| Inventario | **PASS** |
| RBAC / Multitenant | **PASS** smoke + skips seed |
| Responsive | **PASS** |

---

## Veredicto

# **PASS WITH CONDITIONS**

**Condiciones (honestas):**
1. Retest verde de los 4 casos afectados por `ERR_CONNECTION_TIMED_OUT` cuando el VPS/GitHub vuelvan a responder (no son regresiones de producto demostradas).
2. Completar seed mesero/cajero/cross-tenant para eliminar skips.
3. Split de cuenta UI profundo sigue **PARTIAL** (API cubierta).

**Criterios P0 del brief (salida POS, dirty-state, sin 500 en apertura caja, sesiones visibles):** **cumplidos con evidencia real.**

No se declara **PASS absoluto 100/100** mientras existan 4 fails de red en el último run y gaps P2 de seed/cobertura.
