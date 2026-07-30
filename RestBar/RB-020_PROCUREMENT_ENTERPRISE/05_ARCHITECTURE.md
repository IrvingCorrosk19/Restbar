# 05 — ARCHITECTURE

---

# Capas (patrón RB-010)

```
Controllers/
  SupplierController.cs          MVC + API (reusar rutas JS legacy)
  PurchaseRequestController.cs
  PurchaseOrderController.cs
  GoodsReceiptController.cs
  ProcurementReportController.cs
  ProcurementDashboardController.cs

Domain/Procurement/
  PurchaseOrderStateMachine.cs
  PurchaseRequestStateMachine.cs

Models/EnterpriseProcurement.cs
Models/RestBarContext.Procurement.cs

Services/Procurement/
  SupplierService.cs
  PurchaseRequestService.cs
  PurchaseOrderService.cs
  GoodsReceiptService.cs
  ProcurementCostEngine.cs
  SupplierScoreService.cs
  ProcurementApprovalService.cs
  ProcurementReportService.cs
  ProcurementIntegrityService.cs

Infrastructure/Procurement/
  ProcurementHashChainBuilder.cs
  GoodsReceiptInventoryHook.cs   (llama IInventoryOperationsService)

Extensions/EnterpriseProcurementExtensions.cs
  → AddEnterpriseProcurementModule()
```

---

# Integración inventario (crítica)

```
GoodsReceiptService.CompleteAsync
  BEGIN TX
    validate PO state
    for each accepted line:
      IInventoryOperationsService.RestoreStock + LogMovement(Purchase, ...)
        WITH UnitCost, GoodsReceiptId, PurchaseOrderId, SupplierId
      ProcurementCostEngine.ApplyAsync(productId, qty, unitCost)
      PriceHistory.Append
    update PO quantities / status
    SupplierScoreService.Invalidate(supplierId)
    ProcurementAuditEvent
  COMMIT
  SignalR NotifyProcurementReceipt
```

**Regla:** `InventoryOperationsService` / `OrderService` **no** contienen lógica de PO.  
Solo el hook de recepción escribe movimientos Purchase “oficiales”.

`InventoryMovementController.CreatePurchase` (ad-hoc) permanece para emergencias  
pero marca `Reference = "ADHOC"` y alerta si `EnablePurchasingModule` + política RequirePO.

---

# CQRS light

| Write | Read |
|-------|------|
| *Service commands | Dashboard / Reports AsNoTracking |
| Transacciones | Snapshot Command Center cached 30s |

---

# Feature flag

`FeatureFlags.EnablePurchasingModule` default **false**  
`EnableSupplierUi` → true cuando módulo certificado (o alias)

Branch override futuro: `SystemSettings.RequirePurchaseOrderForStockIn`

---

# Background jobs (v1.1)

| Job | Función |
|-----|---------|
| StalePurchaseOrderJob | PO Sent > expected_delivery → alert |
| SupplierScoreRecomputeJob | nightly |
| ReorderSuggestionJob | MinStock − stock → draft PR |

---

# No romper

- OrderService, Payment, Cash (RB-010), KDS, Recipe explosion venta  
- Contracts públicos Payment API  
- Tests foundation + cash 25/25  

---

# Performance targets

| Op | P95 |
|----|-----|
| Create PO (20 lines) | 400ms |
| Complete receipt (20 lines) | 2s |
| Dashboard snapshot | 500ms |
| Cost apply per line | 50ms |
