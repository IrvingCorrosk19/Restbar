# 11 — KITCHEN ROUTING REPORT

## Arquitectura de enrutamiento

```
Orden → ProductStockAssignment → Station (por prioridad/área/piso)
      → OrderItem.PreparedByStationId
      → KDS /api/kitchen/current?stationId=
```

## Pruebas ejecutadas (Routing suite)

| ID | Escenario | Resultado |
|----|-----------|-----------|
| RT-S01-01 | Orden Piso 1 NO aparece en Cocina Piso 2 | PASS |
| RT-S01-02 | Hamburguesa Piso 1 → Parrilla Piso 1 | PASS |
| RT-S02-01 | Pizza → Horno únicamente | PASS |
| RT-S02-03 | Postre → Pastelería únicamente | PASS |
| RT-S09-01 | Pizza prioridad 20 → Horno A, no Horno B | PASS |
| RT-S06-01 | Cambio manual estación Horno → Horno B | PASS |
| RT-S11-01 | KDS idempotente (sin duplicar órdenes) | PASS |

## Múltiples cocinas

| Cocina | Configuración | Estado |
|--------|---------------|--------|
| Cocina Principal | Estaciones por área/piso | PASS |
| Cocina Express | Seed enterprise routing | PARTIAL (sin E2E browser) |
| Cocina VIP | Área Piso 3 | PARTIAL |

**Decisión de ruta:** `ProductService.FindBestStationForProductAsync` evalúa:
1. Asignaciones `ProductStockAssignment` por estación
2. Prioridad numérica (mayor = preferida)
3. Scope área/piso/sucursal

**Cocina inactiva:** Estación `IsActive=false` excluida del routing; fallback a siguiente prioridad.

## SignalR / KDS

- Chef accede `/Order/StationOrders?stationType=kitchen` (NAV-01 PASS)
- Items filtrados por `PreparedByStationId`, no orden completa

## Veredicto cocina

**PASS** para operación multi-piso y multi-estación. Express/VIP requieren validación operativa en piloto.
