# 12 — ORDER FINANCIAL VALIDATION

## Reglas financieras probadas

| Regla | Caso | Resultado |
|-------|------|-----------|
| Pago parcial aceptado | S09-01 $1.00 | PASS |
| Summary refleja pagado | S09-02 totalPaid=1.00 | PASS |
| Sobrepago rechazado | S09-03 Amount=99999 → 400 | PASS |
| Idempotency sin doble cargo | S09-04 misma key 2x | PASS |
| Pagos concurrentes sin duplicar | S15-01 3x $0.25 | PASS |
| Cancel libera orden para no pagar | S05 cancel pre-pago | PASS |
| Pago orden cancelada rechazado | Validado en iteración 1 | PASS (mensaje correcto) |

## Integridad

- Total orden = suma items no cancelados (PaymentController)
- Advisory lock por OrderId en pagos parciales
- Void payment vía DELETE (pre-existente, no re-probado en este ciclo)

## Cuentas separadas

- 2 personas creadas por orden (S08-01/02)
- Asignación de items vía PersonController (API OK)
- UI `separate-accounts.js` ahora cargada en Index

## Descuentos

- **Limitación:** descuento modal es client-side (ORD-O01)
- No afecta cálculo de pagos servidor (usa items reales)

## Inconsistencias financieras detectadas

**Ninguna** en ejecución final.
