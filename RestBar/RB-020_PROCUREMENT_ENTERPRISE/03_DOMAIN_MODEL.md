# 03 — DOMAIN MODEL

---

# Decisión: nuevas entidades vs extender

| Entidad | Decisión | Justificación |
|---------|----------|---------------|
| **Supplier** | CREAR | No existe; JS stub no cuenta |
| **SupplierContact** | CREAR | Contactos múltiples |
| **SupplierProduct** | CREAR | Catálogo aprobado precio/UOM |
| **PurchaseRequest** + Lines | CREAR | Ciclo PR formal |
| **PurchaseOrder** + Lines | CREAR | Documento contractual |
| **GoodsReceipt** + Lines | CREAR | GRN obligatorio |
| **PurchaseApproval** | CREAR | Dual approval / umbrales |
| **SupplierScore** | CREAR | Scorecard snapshot |
| **PriceHistory** | CREAR | Inmutable forense |
| **ProcurementAuditEvent** | CREAR | Hash chain (patrón Cash) |
| **Product** | EXTENDER | `LastPurchaseCost`, `AverageCost`, `LastPurchaseAt` |
| **InventoryMovement** | EXTENDER | `GoodsReceiptId?`, `PurchaseOrderId?`, `UnitCost?`, `SupplierId?` |
| **Recipe** | REUSAR | Sin duplicar; Cost Engine lee |
| **Station** | REUSAR | Ubicación recepción (no Warehouse) |
| **Invoice** (sales) | NO TOCAR | Es venta, no AP |
| **SupplierQuote / Contract / Return** | FASE 1.1 | Diseñados, tablas opcionales v1 mínimas |

---

# Aggregates

```
Supplier (root)
  ├── SupplierContact[]
  ├── SupplierProduct[]
  └── SupplierScore (1:1 current)

PurchaseRequest (root)
  └── PurchaseRequestLine[]

PurchaseOrder (root)
  ├── PurchaseOrderLine[]
  ├── PurchaseApproval[]
  └── GoodsReceipt[]
        └── GoodsReceiptLine[]

PriceHistory (append-only)
ProcurementAuditEvent (append-only hash chain)
```

---

# Enums

```
SupplierStatus: Active, Inactive, OnHold, Blacklisted, Preferred

PurchaseRequestStatus:
  Draft, Pending, Approved, Rejected, Cancelled, Converted, Completed, Audited

PurchaseOrderStatus:
  Draft, PendingApproval, Approved, Sent, PartiallyReceived,
  FullyReceived, Closed, Cancelled, Returned, Audited

GoodsReceiptStatus:
  Draft, InProgress, Completed, Cancelled, Disputed

ReceiptLineDisposition:
  Accepted, Partial, Rejected, Damaged, Short, Over

PurchaseApprovalType: Request, Order, Variance, Emergency, Return
PurchaseApprovalStatus: Pending, Approved, Rejected
```

---

# Invariantes

1. Un PO pertenece a 1 Company + 1 Branch + 1 Supplier  
2. Receipt solo si PO ∈ {Sent, PartiallyReceived, Approved}  
3. Stock solo se mueve al Completar línea Accepted/Partial (AcceptedQty > 0)  
4. Cost Engine corre **después** de Commit receipt, atómica en misma transacción  
5. PriceHistory append-only; nunca UPDATE  
6. Blacklisted supplier → no nuevos PO (override Manager+audit)  
7. CompanyId/BranchId denormalizados en hijos para queries MT  

---

# Relación con inventario

```
GoodsReceipt.Complete
  → foreach AcceptedLine:
       RestoreStock(product, qty, station)
       Log InventoryMovement(Purchase, qty, UnitCost, GoodsReceiptId, SupplierId)
       CostEngine.ApplyReceipt(product, qty, unitCost)
```

**NO** crear movimiento de inventario paralelo. Reusar `IInventoryOperationsService`.
