# 03 — ORDER TEST CASES

## Escenario 1 — Apertura

| ID | Caso | Resultado |
|----|------|-----------|
| S01-01 | GetActiveTables devuelve mesas del tenant | PASS |
| S01-02 | SetTableOccupied en mesa disponible | PASS |
| S01-03 | GetActiveOrder?tableId= sin orden | PASS |
| S01-04 | GetActiveOrder Guid.Empty → 400 | PASS |
| S01-05 | Mesa inexistente → 403 (sin fuga) | PASS |
| S01-06 | 2 SendToKitchen concurrentes misma mesa | PASS |
| S01-07 | GetActiveOrder con orden activa | PASS |

## Escenario 2 — Productos

| S02-01 | Agregar items a orden existente | PASS |
| S02-02 | SendToKitchen sin items → 400 | PASS |
| S02-03 | CheckItemStockAvailability | PASS |
| S02-04 | Cantidad negativa → 400 | PASS |

## Escenarios 3–4

| S03-01 | GetOrderStatus | PASS |
| S03-02 | UpdateItemInOrder notas | PASS |
| S04-01 | Status SentToKitchen | PASS |
| S04-02 | Segundo envío a cocina | PASS |

## Escenario 5 — Cancelación

| S05-01 | Cancel orden dedicada | PASS |
| S05-02 | Sin orden activa post-cancel | PASS |

## Escenario 8 — Cuentas separadas

| S08-01 | CreatePerson x2 | PASS |
| S08-02 | GetPersonsByOrder ≥2 | PASS |

## Escenario 9 — Pagos

| S09-01 | Pago parcial $1.00 | PASS |
| S09-02 | Summary totalPaid=1.00 | PASS |
| S09-03 | Sobrepago rechazado | PASS |
| S09-04 | Idempotency key | PASS |

## Escenario 11 — Cambio mesa

| S11-01 | MoveToTable exitoso | PASS |
| S11-02 | Orden activa en mesa nueva | PASS |
| S11-03 | Mover a mesa ocupada → 400 | PASS |

## Escenarios 13–17

| S13-01..05 | Aislamiento tenant/branch | PASS |
| S14-01..02 | IDOR cancel/payment | PASS |
| S15-01 | 3 pagos paralelos 3/3 | PASS |
| S17-01 | Audit/Index admin | PASS |

## Browser E2E

| BR-01 | Login admin → Order/Index | PASS |
| BR-02 | Mesa T-09 + categoría Bebidas | PASS |
| BR-03 | Agregar Café Americano $2.50 | PASS |
| BR-04 | Modal estación admin + envío cocina | PASS |
| BR-05 | orderId asignado post-envío | PASS |
