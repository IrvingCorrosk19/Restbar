# 07 — FIXES APPLIED

## Backend — Order/Index

| Archivo | Cambio |
|---------|--------|
| `OrderController.cs` | Multitenant guards, GetActiveOrder query binding, ApplyDiscount, active items filter, status string en mesas |
| `OrderService.cs` | MoveToTable, ApplyOrderDiscount, CancelOrderItem supervisor, GetActiveOrder filtros, PreparedByStation include |
| `UserAssignmentService.cs` | Acceso mesero por área |
| `PaymentController.cs` | TipAmount, refund endpoint, descuento en total |
| `PaymentService.cs` | RefundPaymentAsync |
| `RecipeController.cs` | Save recipe idempotente |
| `ShiftController.cs` | Handoff sin throw |
| `StockTransferController.cs` | Approve NotFound HTTP |

## Backend — Infra datos

| Archivo | Cambio |
|---------|--------|
| `RestBarContext.cs` | Mapeos Order discount, Station printer, EnterpriseOperations (8 entidades) |
| `Models/EnterpriseOperations.cs` | Recipe, Shift, Refund, Transfer, etc. |
| `scripts/enterprise-closure-migration.sql` | Tablas/columnas enterprise |

## Frontend

| Archivo | Cambio |
|---------|--------|
| `payments.js` | `updatePaymentInfo()` |
| `discounts.js` | Persistencia vía `ApplyDiscount` |
| `Index.cshtml` | separate-accounts.js, sin debug scripts |

## Certificación

| Script | Cambio |
|--------|--------|
| `Run-OrderCertification.ps1` | Mesa mesero, Test-TableFree |
| `Run-RoutingCertification.ps1` | RT-S09 order-specific |
| `Run-FullCertification.ps1` | Reset mesa POS |
| `Run-EnterpriseCertification.ps1` | 23 casos enterprise |
| `Cert-Common.ps1` | Reset global mesas/órdenes entre suites |
| `Run-AllCertifications.ps1` | Reset inter-suite + exit codes |

**Build:** 0 errores (warnings nullable únicamente)
