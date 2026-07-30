# 02 — REQUIREMENTS

---

# Functional (MUST)

| ID | Requisito |
|----|-----------|
| FR-01 | CRUD Supplier multi-tenant con contactos, términos, lead time, estado, blacklist |
| FR-02 | Catálogo SupplierProduct (producto ↔ proveedor ↔ precio acordado ↔ UOM/pack) |
| FR-03 | Purchase Request: Draft→Pending→Approved/Rejected→Converted |
| FR-04 | Purchase Order: ciclo completo + líneas + estados documentados |
| FR-05 | Goods Receipt parcial/total con diferencias, daño, rechazo, lotes, vencimiento |
| FR-06 | Receipt → InventoryMovement.Purchase + stock Station/Product |
| FR-07 | Cost Engine: LastCost + MovingAverageCost → Product.Cost |
| FR-08 | PriceHistory inmutable por producto/proveedor |
| FR-09 | SupplierScore automático (precio, OTIF, calidad, rechazos) |
| FR-10 | Dual approval por umbral de monto |
| FR-11 | Procurement Command Center (<5s) |
| FR-12 | Feature flag EnablePurchasingModule (default false) |
| FR-13 | Reusar JS supplier-management donde sea viable |
| FR-14 | Conectar SupplierAnalysis a datos reales |
| FR-15 | Theoretical food cost desde Recipe × ingredient Cost |
| FR-16 | Alerts: stock bajo, PO atrasado, precio ↑, proveedor blacklist |

# Functional (SHOULD — v1.1)

| ID | Requisito |
|----|-----------|
| FR-20 | SupplierQuote / comparación cotizaciones |
| FR-21 | PurchaseReturn + credit note |
| FR-22 | Three-way match vs SupplierInvoice (AP) |
| FR-23 | Foto recepción / firma digital |
| FR-24 | Background jobs: stale PO, reorder suggestions |

# Non-functional

| ID | Requisito |
|----|-----------|
| NFR-01 | Tenant isolation Company/Branch en toda entidad |
| NFR-02 | Sin N+1; índices en lookups críticos |
| NFR-03 | AsNoTracking en dashboards |
| NFR-04 | Receipt de 50 líneas < 3s P95 |
| NFR-05 | Audit hash chain (patrón RB-010) |
| NFR-06 | 0 regresiones Orders/Payments/Cash/KDS |
| NFR-07 | Policy PurchasingAccess / CostingAccess |

# Out of scope v1

OCR invoices · EDI · Accounting GL sync · Franchise royalties · Marketplace
