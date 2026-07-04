# 15 — AUDIT REPORT

## Capacidades de auditoría

| Evento | Registro | Evidencia |
|--------|----------|-----------|
| Creación orden | `Order.OpenedAt`, `AddedByUserId` | S01-06 |
| Modificación ítem | `OrderItem` timestamps, notas | S03-02 |
| Envío cocina | Status `SentToKitchen`, estación | S04-01 |
| Cancelación orden | `OrderCancellationLog` | S05-01 |
| Cancel ítem cocina | Supervisor required | ENT-CANC-01/02 |
| Pago | `Payment` + `IsVoided` | S09-01 |
| Reembolso | `PaymentRefund` | ENT-REF-01 |
| Descuento | `DiscountAmount/Type/Reason` | ENT-DISC |
| Cambio mesa | `MoveToTable` audit log | S11-01 |
| Turno mesero | `Shift`, `ShiftTableHandoff` | ENT-SHIFT |
| Inventario | `InventoryMovement` | ENT-INV |
| Transferencia | `StockTransfer` approve trail | ENT-XFER |

## Acceso forense

- `GET /Audit/Index` — admin (S17-01 PASS)
- `GlobalLoggingService` en operaciones críticas Order/Payment
- Campos tenant: `CompanyId`, `BranchId` en entidades principales

## Campos pendientes fase 2

| Campo | Estado |
|-------|--------|
| IP del cliente | No persistido sistemáticamente |
| Equipo/terminal ID | No modelado |
| Quién imprimió | Sin log impresión térmica |

## Preguntas forenses (escenario 26)

| Pregunta | ¿Responde? |
|----------|------------|
| Quién creó la orden | Sí — `AddedByUserId` |
| Quién modificó | Sí — logs + timestamps |
| Quién canceló | Sí — `OrderCancellationLog` |
| Quién cobró | Sí — `Payment.ProcessedBy` |
| Quién aplicó descuento | Sí — `ApplyDiscount` con usuario |
| Quién autorizó devolución | Sí — `PaymentRefund` |
| Empresa/Sucursal/Mesa | Sí — FK en entidades |
| Hora exacta | Sí — UTC timestamps |

## Veredicto auditoría

**PASS** para operación enterprise estándar. IP/terminal en fase 2.
