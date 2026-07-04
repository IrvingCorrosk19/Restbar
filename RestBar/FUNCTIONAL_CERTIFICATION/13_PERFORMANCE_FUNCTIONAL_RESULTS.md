# Resultados de Rendimiento Funcional

**Fecha:** 2026-07-04  
**Ambiente:** Desarrollo local (single instance)

## Pruebas de carga ejecutadas

| Prueba | Resultado |
|--------|-----------|
| Suite 32 tests secuenciales | ~15s total ✅ |
| 12 logins consecutivos | Sin 429 tras fix rate limiter ✅ |
| Seed multi-tenant | < 2s ✅ |

## Pruebas NO ejecutadas (fase 2)

| Escenario | Objetivo |
|-----------|----------|
| 1.000 pedidos | Tiempo creación < 5s/pedido |
| 1.000 productos | Carga catálogo POS < 3s |
| 5 sesiones simultáneas | Sin inconsistencia de mesas |
| 10 pagos concurrentes misma orden | Un solo cobro efectivo |

## Observaciones

- Rate limiter Development: 500 req/min — adecuado para certificación, revisar para producción
- Índice `idx_unique_active_order_per_table` previene duplicados bajo concurrencia ligera
- Advisory lock en pagos soporta concurrencia moderada

## Veredicto rendimiento

**PASS condicional** — Sin degradación en flujo normal. Pruebas de estrés pendientes para producción.
