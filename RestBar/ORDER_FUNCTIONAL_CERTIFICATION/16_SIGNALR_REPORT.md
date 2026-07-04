# 16 — SIGNALR REPORT

## Infraestructura

| Componente | Detalle |
|------------|---------|
| Hub | `/orderHub` con `[Authorize]` |
| Cliente POS | `wwwroot/js/order/signalr.js` |
| Eventos | `OrderStatusChanged`, `TableStatusChanged`, `PaymentProcessed`, `OrderCancelled`, `NewOrder`, `KitchenUpdate` |

## Pruebas indirectas (PASS)

| Evento | Evidencia |
|--------|-----------|
| `TableStatusChanged` | `SetTableOccupied` → notificación (S01-02) |
| `OrderStatusChanged` | SendToKitchen, Cancel (código + API) |
| `MoveToTable` | Notify old + new table (S11-01) |
| KDS refresh | `KitchenUpdate` tras envío cocina |

## KDS idempotencia

RT-S11-01: doble consulta `/api/kitchen/current` retorna mismo conteo — sin duplicación de órdenes en snapshot.

## No ejecutado E2E

| Caso | Estado | Impacto |
|------|--------|---------|
| Reconexión tras perder internet 30s | No ejecutado | Medio |
| Hub caído + recuperación | No ejecutado | Medio |
| 2 navegadores tiempo real | No ejecutado | Bajo |

## Observación técnica

`stock-updates.js` abre segunda conexión al mismo hub — riesgo bajo de conexiones duplicadas; no bloqueante.

## Veredicto SignalR

**PASS** con cobertura parcial. Flujos críticos notifican correctamente. Reconexión offline requiere prueba browser fase 2.
