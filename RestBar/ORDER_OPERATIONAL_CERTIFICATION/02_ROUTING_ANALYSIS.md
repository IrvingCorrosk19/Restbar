# 02 — Routing Analysis

## Mecanismo

```
SendToKitchen → FindBestStationForProductAsync(productId, qty, branchId, tableAreaId)
              → ProductStockAssignment (activas, por branch)
              → Preferencia área mesa == Station.AreaId
              → Orden: Priority DESC, Stock DESC (si TrackInventory)
              → OrderItem.PreparedByStationId = stationId
```

## Matriz validada (39 tests OP-RTE-*)

| Producto | Estación destino | Test |
|----------|------------------|------|
| Pizza Enterprise | Horno | OP-RTE-01 PASS |
| Hamburguesa Enterprise | Parrilla | OP-RTE-02 PASS |
| Cerveza Enterprise | Bar Principal | OP-RTE-03 PASS |
| Postre Enterprise | Pastelería | OP-RTE-04 PASS |
| Ensalada Enterprise | Cocina Fría | OP-RTE-05 PASS |
| Sopa Enterprise | Cocina Caliente | OP-RTE-06 PASS |
| Pasta Alfredo | Cocina Express | OP-MK-01 PASS |
| Trago VIP | Bar VIP (no Bar Principal) | OP-MB-01/02 PASS |

## Orden mixta

Una sola orden con 6 productos se **divide automáticamente** — cada estación recibe solo sus ítems (OP-RTE-07 PASS).

## Aislamiento por piso

Orden Piso 2 no aparece en Cocina Piso 2; hamburguesa va a Parrilla Piso 1 (OP-FLR-01/02 PASS).

## Limitación documentada

`MoveToTable` cross-piso **no re-asigna** estación automáticamente (comportamiento conocido).
