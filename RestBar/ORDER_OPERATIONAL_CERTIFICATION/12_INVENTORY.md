# 12 — Inventory

## Cuándo descuenta

| Momento | Comportamiento |
|---------|----------------|
| SendToKitchen | `DeductInventoryForSaleAsync` si `TrackInventory=true` |
| ProductStockAssignment.Stock | Descuenta stock por estación cuando aplica |
| Cancelación pre-cocina | Sin descuento (ítem no enviado) |
| Cancelación post-cocina | Restauración según regla supervisor |

## Validación OP

| ID | Resultado |
|----|-----------|
| OP-INV-01 | Routing vía PSA (14 asignaciones), no `Product.StationId` | PASS |

## Nota seed enterprise

Productos Enterprise tienen `TrackInventory=false` — routing por prioridad sin agotamiento de stock. Failover stock→estación B validado en diseño de `FindBestStationForProductAsync`.
