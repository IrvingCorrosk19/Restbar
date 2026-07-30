# 11 — CASH MOVEMENTS

---

# Enum CashMovementType

| Type | Direction | Affects Drawer | Origen | Auto/Manual |
|------|-----------|----------------|--------|-------------|
| OpeningFloat | In | ✅ | Apertura | Auto |
| SaleCash | In | ✅ | Payment efectivo | Auto |
| SaleCard | In | ❌ | Payment tarjeta | Auto |
| SaleYappy | In | ❌ | Payment Yappy | Auto |
| SaleACH | In | ❌ | Payment ACH/transfer | Auto |
| SaleOther | In | ❌ | Otros métodos | Auto |
| TipCash | In | ✅ | Tip en efectivo | Auto |
| TipNonCash | In | ❌ | Tip tarjeta | Auto |
| ChangeGiven | Out | ✅ | Cambio entregado | Auto |
| PaidIn | In | ✅ | Ingreso manual | Manual |
| PaidOut | Out | ✅ | Retiro/gasto menor | Manual |
| PettyPurchase | Out | ✅ | Compra menor | Manual |
| RefundCash | Out | ✅ | PaymentRefund | Auto |
| VoidReversal | Out/In | ✅/❌ | Void payment | Auto |
| AdjustmentIn | In | ✅ | Corrección supervisor | Manual+Approval |
| AdjustmentOut | Out | ✅ | Corrección supervisor | Manual+Approval |
| DropToSafe | Out | ✅ | Retiro parcial cierre | Manual |
| DepositBank | Out | ✅ | Depósito banco | Manual v1.1 |
| TransferOut | Out | ✅ | A otra register | v2 |
| TransferIn | In | ✅ | Desde otra register | v2 |
| ConciliationDiff | In/Out | ✅ | Ajuste post-arqueo | Auto+Approval |
| SessionClose | — | — | Marker event | Auto |
| ReopenMarker | — | — | Marker | Auto |

---

# Campos obligatorios por movimiento

| Campo | Req |
|-------|-----|
| Id | ✅ |
| CashSessionId | ✅ |
| MovementType | ✅ |
| Direction | ✅ |
| Amount | ✅ > 0 |
| PerformedByUserId | ✅ |
| CreatedAtUtc | ✅ |
| CompanyId, BranchId | ✅ |
| SequenceNumber | ✅ |
| RecordHash | ✅ |
| ReasonCode | Manual/Adjustment |
| AuthorizedByUserId | PaidOut high, Adjustment |
| PaymentId | Auto from payment |
| IdempotencyKey | Auto payment |

---

# Mapeo Payment.Method → MovementType

| Payment.Method (case insensitive) | Movement |
|-----------------------------------|----------|
| Efectivo, Cash, Efectivo USD | SaleCash |
| Tarjeta, Card, Visa, MC | SaleCard |
| Yappy | SaleYappy |
| Transferencia, ACH, Wire | SaleACH |
| Mixto | Split per SplitPayment lines |

---

# Inmutabilidad y reversos

- **Prohibido** UPDATE amount  
- Void payment → nuevo movement `VoidReversal` linked `RelatedMovementId`  
- Refund → `RefundCash` Out  
- Correction → pair AdjustmentOut + AdjustmentIn con approval  

---

# Propinas

`TipAmount` on Payment → if cash sale, `TipCash` movement; else `TipNonCash`. Settlement report at close uses `TipAllocation` for staff distribution (no payroll v1).
