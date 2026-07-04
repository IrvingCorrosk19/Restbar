# 13 — ORDER SIGNALR VALIDATION

## Infraestructura

- Hub: `/orderHub` con `[Authorize]`
- Cliente: `wwwroot/js/order/signalr.js`
- Eventos: OrderStatusChanged, TableStatusChanged, PaymentProcessed, OrderCancelled, NewOrder, KitchenUpdate

## Pruebas indirectas (PASS)

| Evento | Evidencia |
|--------|-----------|
| TableStatusChanged | Mesa T-09 → Ocupada tras selección (browser) |
| SetTableOccupied | API notifica SignalR (OrderController) |
| MoveToTable | NotifyTableStatusChanged old + new |
| PaymentProcessed | Toast handler en signalr.js (código verificado) |

## Pruebas no E2E en esta fase

| Caso | Estado |
|------|--------|
| Reconexión tras perder internet | No ejecutado |
| Reinicio hub con 2 navegadores | No ejecutado |
| KDS recibe NewOrder en tiempo real | Parcial (envío cocina OK, KDS no abierto en browser) |

## Dual connection

- `stock-updates.js` abre segunda conexión al mismo hub — riesgo bajo, no bloqueante

## Veredicto SignalR

Sin errores en flujos probados. Reconexión E2E pendiente para certificación extendida.
