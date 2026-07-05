# 05 — Kitchen Validation

## KDS / Kitchen Queue

- **Vista:** `GET /Order/StationOrders?stationType=kitchen&stationId=`
- **API:** `GET /api/kitchen/current?stationId=`
- **Filtro:** Solo ítems con `PreparedByStationId` en estaciones del usuario/tenant

## Tests PASS

| ID | Validación |
|----|------------|
| OP-KDS-01 | Marcar preparing |
| OP-KDS-02/03 | MarkItemReady → KitchenStatus=Ready |
| OP-KDS-05 | Orden ReadyToPay cuando todos listos |
| OP-KDS-06 | Snapshot KDS idempotente |
| OP-MK-01 | Cocina Express recibe pasta exclusiva |
| OP-FLR-01 | Cocina Piso 2 no recibe órdenes Piso 1 |
| OP-UI-01 | StationOrders kitchen carga (HTTP 200) |
| OP-CON-01 | Doble MarkItemReady concurrente sin corrupción |

## Múltiples cocinas

Cocina Principal, Caliente, Fría, Express, Pastelería, Horno, Parrilla — cada una recibe **solo** sus productos asignados.
