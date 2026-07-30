# 24 — Certificación funcional (parcial honest)

## Build
| Item | Resultado |
|------|-----------|
| `dotnet build -c Release` | **PASS** — 0 errors |
| Deploy VPS commit | `14e12aa` (+ JS sync fix pushed) |
| HTTP `/Auth/Login` | 200 |

## Defecto P0 navegación
| Item | Resultado |
|------|-----------|
| Causa raíz documentada | `01_NAVIGATION_ROOT_CAUSE.md` |
| Fix enterprise | `02_NAVIGATION_FIX.md` |
| Browser tab evidencia | POS muestra Volver/Inicio; Inicio → `/Home` |
| Playwright ORD-NAV-01..06 | **6/6 PASS** |

## Pruebas Playwright (ejecutadas de verdad)
| Suite | PASS | FAIL |
|-------|------|------|
| Orders navigation | 6 | 0 |
| Inventory (prev) | 8 | 0 |

## Alcance NO cerrado en este ciclo (transparencia)
Las fases 4–19 del brief (matriz completa de pisos/estaciones/meseros/caja/cierre/RBAC/multitenant/responsive) **no** se certificaron end-to-end en esta entrega.

Estado de módulos:

| Módulo | Estado |
|--------|--------|
| Pedidos — salir a Home | **PASS** (P0 cerrado) |
| Pedidos — dirty-state | **PASS parcial** (borrador sin orderId + beforeunload) |
| Pisos / estaciones / meseros E2E | **NO EJECUTADO** (suite pendiente) |
| Pagos / split / transfer | **NO EJECUTADO** |
| Caja apertura/cambio/cierre | **NO EJECUTADO** (RB-010 existe; no re-certificado aquí) |
| Turnos | **NO EJECUTADO** |
| Inventario impacto por venta | **PARCIAL** (suite Inventory previa) |
| RBAC / Multitenant matriz completa | **NO EJECUTADO** |
| Responsive matrix | **NO EJECUTADO** (chrome POS tiene CSS móvil) |

## Defectos
| Severidad | Abiertos |
|-----------|----------|
| P0 | **0** (navegación POS corregida) |
| P1+ | Pendientes de las fases no ejecutadas — no inventados |

## Veredicto
**PASS WITH CONDITIONS**

Condiciones:
1. P0 de navegación POS corregido, desplegado y verificado con Playwright + browser.
2. Certificación completa de turno (caja/cierres/pisos/estaciones/meseros) requiere ciclo dedicado de suites Fases 4–19.

## Evidencia
- Playwright: `tests/Browser/Orders/orders-navigation.spec.js`
- Docs: `ORDER_CASH_FUNCTIONAL_CERTIFICATION/01_*.md`, `02_*.md`
- VPS: `http://164.68.99.83:8084/Order/Index`
