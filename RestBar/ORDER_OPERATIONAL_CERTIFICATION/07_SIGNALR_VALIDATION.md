# 07 — SignalR Validation

## Eventos emitidos (código)

- Orden enviada a cocina (`SendToKitchen`)
- Ítem preparing / ready (`UpdateItemStatus`, `MarkItemReady`)
- Cancelación orden/ítem
- Cambio de mesa
- Pago registrado

## Validación automatizada

| ID | Método | Resultado |
|----|--------|-----------|
| OP-REC-01 | Doble `GET /api/kitchen/current` (reconexión API) | PASS |
| OP-KDS-06 | Snapshot idempotente post-ready | PASS |

## Limitación

No se ejecutó prueba browser E2E de latencia SignalR en esta suite. La API KDS (`/api/kitchen/current`) soporta recuperación tras desconexión — patrón usado por KDS frontend.

## Aislamiento

Filtrado por `BranchId`/`CompanyId` en hub groups (validado en ORDER_ROUTING RT-S15).
