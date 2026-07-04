# 14 — FINANCIAL VALIDATION

## Reglas financieras probadas

| Regla | Caso | Resultado |
|-------|------|-----------|
| Pago parcial aceptado | S09-01, PAY-01 $1.00 | PASS |
| Summary refleja pagado | S09-02 totalPaid=1.00 | PASS |
| Sobrepago rechazado | S09-03, PAY-04 → 400 | PASS |
| Idempotencia sin doble cargo | S09-04, PAY-03 | PASS |
| Pagos concurrentes | S15-01 3×$0.25 | PASS |
| Descuento fijo servidor | ENT-DISC-01 $2.00 | PASS |
| Descuento porcentaje | ENT-DISC-02 10% | PASS |
| Propina en pago | ENT-TIP-01 TipAmount | PASS |
| Reembolso parcial | ENT-REF-01 $0.50 | PASS |
| Cancel pre-pago | S05-01 | PASS |

## Integridad

- Total orden = Σ(items activos) − `DiscountAmount`
- Advisory lock PostgreSQL por `OrderId` en pagos
- Pagos voided al cancelar orden (`CancelOrderAsync`)
- `TipAllocation` persistido por pago (ENT-TIP-02)

## Cuentas separadas

- 2 personas por orden (S08-01/02)
- API `PersonController` operativa
- UI `separate-accounts.js` cargada en Index

## Impuestos / redondeos

- `TaxRate` por producto aplicado en cálculo item
- Sin motor fiscal multi-jurisdicción (fase 2)

## Inconsistencias detectadas

**Ninguna** en ejecución final 119/119.

## Veredicto financiero

**PASS** — Pagos, descuentos, propinas y reembolsos consistentes.
