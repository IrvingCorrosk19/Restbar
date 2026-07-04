# Limitaciones y alcance — Motor de enrutamiento

## Modelo actual

- **Piso** = modelado como `Area` (ej. "Piso 1 - Salón", "Piso 2 - Salón")
- **Estación** = `Station` con `Type` (kitchen, bar, grill, oven, pastry…) + `AreaId` + `BranchId`
- **Enrutamiento** = `ProductStockAssignment` (producto → estación, prioridad, stock por sucursal)
- **KDS** = `/Order/StationOrders?stationId=` o `?stationType=&areaId=`
- **Recuperación** = `GET /api/kitchen/current?stationId=`

## Escenarios NO ejecutados al 100%

| # | Escenario | Estado | Motivo |
|---|-----------|--------|--------|
| 4 | Dos cocinas (Principal vs Express) | Parcial | Cocina Express en seed; no probado aislamiento chef dedicado en browser |
| 5 | Cocina compartida entre pisos | No modelado | Requiere estación con `AreaId` null + regla explícita |
| 7 | Cambio de piso (mesa) | Parcial | `MoveToTable` existe; re-enrutamiento por área no automático post-movimiento |
| 8 | Producto sin estación | Corregido | SendToKitchen **rechaza** — no se pierde silenciosamente |
| 10 | Saturación 150 órdenes | No ejecutado | Smoke test 15 casos; carga masiva pendiente |
| 12 | Cancelación post-cocina | Parcial | `CancelOrderItemAsync` restaura stock; SignalR parcial |
| 13 | Cambio pizza→pasta | Parcial | Requiere flujo UI cancel+add; API soporta |
| 14 | Cocina sin conexión | No E2E | `/api/kitchen/current` permite recuperación; reconexión SignalR no probada |

## Riesgos residuales (baja severidad)

1. **Admin sin BranchId** ve KDS global (by design)
2. **Estación compartida entre pisos** requiere configuración manual de `AreaId` en estación
3. **MoveToTable** no reasigna estaciones de ítems pendientes automáticamente

## ¿Soporta restaurante Enterprise?

**Sí, con las correcciones aplicadas**, para:
- Múltiples pisos (vía Áreas)
- Múltiples cocinas/bares/estaciones especializadas
- División de ítems por estación en una misma orden
- Aislamiento multitenant en KDS
- Prioridad determinística (no aleatoria) en multi-estación

**Requiere fase 2** para: carga 150+ órdenes simultáneas, offline prolongado, cocina compartida cross-piso automatizada.
