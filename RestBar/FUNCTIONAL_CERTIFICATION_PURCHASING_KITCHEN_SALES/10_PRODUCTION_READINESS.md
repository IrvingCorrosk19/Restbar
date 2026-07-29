# 10 — PRODUCTION READINESS

**Fecha:** 2026-07-28  
**Alcance:** Órdenes de Compra · Cocina · Ventas

---

## Scorecard

| Módulo | ¿Listo producción? | Preparación funcional estimada | Bloqueadores |
|--------|--------------------|--------------------------------|--------------|
| Órdenes de Compra | **NO** | **~7%** | Módulo inexistente |
| Cocina / KDS | **SÍ (piloto)** | **~88%** | Offline stress, print térmica, re-routing mesa |
| Ventas / POS | **PARCIAL** | **~72%** operación continua · **~85%** POS core | Caja, fiscal, combos, HH |
| **Conjunto 3 módulos** | **NO** | **~55%** | PO + caja + fiscal |

---

## Criterios go/no-go

| Criterio | Estado |
|----------|--------|
| Flujos esenciales completos en los 3 módulos | **NO** (Compras 0%) |
| 0 defectos Critical/High abiertos | **NO** |
| Integridad financiera (caja) | **NO** |
| Integridad inventario compras | **NO** (solo stock-in ad-hoc) |
| Cocina multi-estación operable | **SÍ** |
| POS mesa→pago operable | **SÍ** |

---

## Recomendación

| Uso | Decisión |
|-----|----------|
| Piloto POS+KDS asistido (sin compras ni fiscal) | Condicional **SÍ** |
| Producción enterprise con compras | **NO** |
| Venta SaaS self-service | **NO** |
| Prometer módulo Compras | **NO** |

---

## Obligatorio antes de piloto con cliente real

1. Declarar explícitamente que **Compras/Proveedores no están incluidos**.
2. Implementar o externalizar **cierre de caja** mínimo (apertura, arqueo, diferencia).
3. Definir estrategia **precuenta / factura** (aunque sea integración fiscal local).
4. Onboarding de mesas/asignaciones mesero (evitar 403 operativos).
5. Runbook SignalR + snapshot `/api/kitchen/current`.
6. UAT en staging con datos del cliente (estaciones PSA reales).
