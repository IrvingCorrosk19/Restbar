# 04 — Order Lifecycle

## Estados de orden

```
Pending → SentToKitchen → Preparing → Ready → ReadyToPay → Served → Completed
                                                              ↓
                                                         Cancelled
```

## Estados de ítem

| Campo | Flujo |
|-------|-------|
| `KitchenStatus` | Pending → Sent → Ready → Cancelled |
| `OrderItemStatus` | Pending → Preparing → Ready → Served → Cancelled |

## Validación ejecutada

| Paso | Test | Resultado |
|------|------|-----------|
| Crear orden + notas | OP-ORD-01 | PASS |
| Modificar cantidad/notas | OP-ORD-02 | PASS |
| Agregar producto | OP-ORD-03 | PASS |
| Chef: preparing | OP-KDS-01 | PASS |
| Chef: mark ready | OP-KDS-02/03 | PASS |
| Bartender: mark ready | OP-KDS-04 | PASS |
| Todos listos → ReadyToPay | OP-KDS-05 | PASS |
| Pago parcial cajero | OP-PAY-01 | PASS |

## Transiciones imposibles

No se detectaron estados inconsistentes en la suite de 39 casos.
