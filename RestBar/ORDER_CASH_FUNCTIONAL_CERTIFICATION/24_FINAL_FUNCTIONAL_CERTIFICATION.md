# 24 — Certificación funcional final (Order + Cash + KDS)

**Fecha:** 2026-07-30  
**Ambiente:** VPS `http://164.68.99.83:8084`  
**Commit suite:** `7988f16` (+ cash open `eebc419`)

---

## Build

| Item | Resultado |
|------|-----------|
| `dotnet build -c Release` | **PASS** — 0 errors |
| Deploy VPS | **OK** |
| Apertura caja real | **PASS** → Sesión #1 Open |

---

## Pruebas Playwright (chromium-desktop) — corrida final

| Métrica | Valor |
|---------|-------|
| Planificadas | 78 |
| Ejecutadas | 78 |
| **PASS** | **77** |
| **FAIL** | **0** |
| **SKIPPED** | **1** (MT-02 cross-tenant seed ausente) |
| Duración | 22.5 min |
| Exit code | **0** |

Suites: Orders · Floors · Stations · Waiters · Tables · Kitchen · Payments · Cash · Shifts · Inventory · Responsive · Negatives · Multitenant · Operations

Evidencia: `RB-010_020_023_BROWSER_CERTIFICATION/evidence/test-output/` · `playwright-results.json`

---

## Defectos

| Severidad | Abiertos |
|-----------|----------|
| **P0** | **0** |
| **P1** | **0** |
| P2 | MT-02 seed; split UI profundo (API OPS-02 cubierta) |

Cerrados en ciclo: DEF-NAV-001, DEF-CASH-ROWVER-001, DEF-CASH-OPEN-001, DEF-CASH-DASH-001, DEF-ORD-STATUS-001, DEF-POS-SWAL-001.

---

## Módulos

| Módulo | Veredicto |
|--------|-----------|
| Pedidos (salida + E2E) | **PASS** |
| Pisos / estaciones / mesas / meseros | **PASS** |
| KDS | **PASS** |
| Pagos | **PASS** |
| Caja (apertura, dashboard, arqueo, paid-in, doble open) | **PASS** |
| Cancel / split API / transfer | **PASS** |
| Inventario | **PASS** |
| Turnos / responsive / negativos | **PASS** |
| Multitenant | **PASS** (1 skip seed) |

---

## Veredicto

# **PASS**

Suite browser Order+Cash del brief: **0 FAIL**. Único skip: MT-02 por usuario cross-tenant no sembrado (P2 seed, no defecto de producto).

Criterios P0 (salida POS, dirty-state, sin 500 en caja, sesiones visibles, mesa→cocina, KDS, RBAC smoke): **cumplidos con evidencia real en VPS.**
