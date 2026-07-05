# 08 — Concurrency

## Escenarios ejecutados

| ID | Escenario | Resultado |
|----|-----------|-----------|
| OP-CON-01 | 2 chefs paralelos MarkItemReady mismo ítem | PASS (jobs=2, ready=True) |

## Comportamiento esperado

- MarkItemReady idempotente — segundo click no corrompe estado
- Ítem queda `KitchenStatus=Ready` una sola vez

## Escenarios no en OP suite (cubiertos en Order cert)

- 3 cajeros pagos paralelos (S15-01)
- 2 meseros misma mesa (S01-06)
- IdempotencyKey pagos (S09-04)

## Conclusión

Sin race conditions críticas detectadas en ciclo cocina listo.
