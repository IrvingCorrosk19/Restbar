# 07 — Validación Flujo de Orden

**Fecha:** 2026-07-05  
**Resultado:** PASS (3 empresas)

## Flujo ejecutado por empresa

Para **Costa**, **Norte** y **Sur**:

| Paso | Acción | TC3 ID | Estado |
|------|--------|--------|--------|
| 1 | Login mesero | ENV-02..04 | PASS |
| 2 | Seleccionar mesa asignada/libre | implícito | PASS |
| 3 | Crear orden + producto (Hamburguesa) | ORD-*-01 | PASS |
| 4 | Observación en ítem | SendToKitchen Notes | PASS |
| 5 | Enviar a cocina | ORD-*-01 | PASS |
| 6 | Ítem con estación | RTE-*-01 | PASS |
| 7 | Crear 2 personas (split) | SPL-*-01 | PASS |
| 8 | Pago parcial cajero | PAY-*-01 | PASS |
| 9 | Cambio de mesa | MOV-*-01 | PASS |
| 10 | Cleanup cancelación | implícito | PASS |

## Pasos validados en suites complementarias

| Paso | Suite | Estado |
|------|-------|--------|
| Chef marca listo | Order + Enterprise | PASS |
| Pago completo + mesa libre | Order cert | PASS |
| Inventario descontado | Enterprise INV | PASS |
| Auditoría | TC3-AUD-01 + Enterprise | PASS |

## Conclusión

El flujo operativo principal funciona en las 3 empresas de forma independiente y consistente.
