# 13 — AUDIT MODEL

---

# ProcurementAuditEvent (append-only + hash chain)

Campos: CompanyId, BranchId, EntityType, EntityId, EventType, ActorUserId, ActorRole,  
BeforeJson, AfterJson, IpAddress, DeviceId, PreviousEventHash, EventHash, CreatedAtUtc

---

# Eventos obligatorios

| EventType | Cuándo |
|-----------|--------|
| SupplierCreated/Updated/Blacklisted | master data |
| PurchaseRequestSubmitted/Approved/Rejected | PR |
| PurchaseOrderSubmitted/Approved/Sent/Cancelled/Closed | PO |
| GoodsReceiptCompleted | GRN |
| CostUpdated | Cost Engine |
| PriceOverride | precio ≠ agreed |
| EmergencyPurchase | ad-hoc stock in con módulo ON |
| ApprovalResolved | dual approval |

---

# Hash

SHA-256 payload canónico (mismo patrón `CashHashChainBuilder`).  
Verify API: `/api/ProcurementReport/verify/{entityId}`

---

# Retención

Nunca borrar. Soft-delete entidades negocio; audit permanente.
