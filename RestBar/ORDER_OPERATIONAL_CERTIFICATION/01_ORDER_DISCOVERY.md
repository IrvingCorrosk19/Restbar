# 01 — Order Engine Discovery

**Fecha:** 2026-07-05  
**Alcance:** Orders, Kitchen, KDS, Stations, Bar (exclusivo)

## Arquitectura del motor

| Capa | Componente | Rol |
|------|------------|-----|
| POS | `OrderController.SendToKitchen` | Entrada de órdenes |
| Servicio | `OrderService.AddOrUpdateOrderWithPendingItemsAsync` | Crea ítems + asigna estación |
| Routing | `ProductService.FindBestStationForProductAsync` | Algoritmo de mejor estación |
| Config | `ProductStockAssignment` | Producto → Estación (prioridad, stock) |
| KDS | `OrderService.GetKitchenOrdersAsync` | Cola cocina/bar |
| Vista | `StationOrders.cshtml` + `KitchenApiController` | Display y API JSON |
| Tiempo real | `IOrderHubService` | SignalR en envío/listo/cancel |

## Regla de enrutamiento (descubierta, no hardcoded)

1. Override manual `SelectedStationId` (admin)
2. Estación del mesero (`UserAssignment.StationId`)
3. **ProductStockAssignment** por producto + sucursal + área de mesa

Resultado persistido en `OrderItem.PreparedByStationId`.

## Configuración operativa validada (OP-CFG)

- 15 estaciones activas
- 14 asignaciones producto-estación
- Mesas P1-01, P2-01, P3-01 en 3 pisos
- Seed: `GET /Seed/SeedEnterpriseRouting`

## Conclusión

El comportamiento está **gobernado por configuración en BD**, no por strings mágicos en código de negocio.
