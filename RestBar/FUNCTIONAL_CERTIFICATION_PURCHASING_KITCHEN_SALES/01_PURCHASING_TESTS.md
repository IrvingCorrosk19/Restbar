# 01 — PURCHASING TESTS (Órdenes de Compra)

**Suite:** `scripts/Run-PurchasingKitchenSalesCertification.ps1`  
**Ejecución:** 2026-07-28 19:06:12 · `http://localhost:5001`  
**Resultado módulo:** **FAIL / NOT IMPLEMENTED** — 3 PASS (sustitutos) · 0 FAIL · **42 BLOCKED**

---

## Veredicto de módulo

El ciclo **Proveedor → OC → Aprobación → Recepción → Inventario → Cierre** **no existe** en RestBar.

| Capa | Estado |
|------|--------|
| Entidad `PurchaseOrder` / `Supplier` / `GoodsReceipt` | Ausente |
| Controller `/PurchaseOrder/*`, `/Supplier/*` | HTTP **404** verificado |
| UI Orden de Compra | Ausente |
| Sustituto | `POST /InventoryMovement/CreatePurchase` (entrada de stock inmediata, sin workflow) |
| Reportes | `SupplierAnalysis` stub con ceros |

---

## Matriz de escenarios (prompt master)

| ID | Escenario | Resultado | Evidencia |
|----|-----------|-----------|-----------|
| PO-01..12 | Endpoints PO/Supplier/Receiving | **BLOCKED** | HTTP 404 |
| PO-SC-01 | Crear orden | **BLOCKED** | Sin entidad |
| PO-SC-02 | Editar antes de aprobar | **BLOCKED** | Sin entidad |
| PO-SC-03 | Aprobar | **BLOCKED** | Sin entidad |
| PO-SC-04 | Rechazar | **BLOCKED** | Sin entidad |
| PO-SC-05 | Cancelar | **BLOCKED** | Sin entidad |
| PO-SC-06 | Eliminar | **BLOCKED** | Sin entidad |
| PO-SC-07 | Reabrir | **BLOCKED** | Sin entidad |
| PO-SC-08 | Duplicar | **BLOCKED** | Sin entidad |
| PO-SC-09 | Orden parcial | **BLOCKED** | Sin entidad |
| PO-SC-10..11 | Recepción parcial/total | **BLOCKED** | Sin entidad |
| PO-SC-12..18 | Diferencias, impuestos, moneda, descuento | **BLOCKED** | Sin entidad |
| PO-SC-19..29 | Duplicada, fuera de tiempo, permisos, producto inválido, inventario bloqueado | **BLOCKED** | Sin entidad |
| PO-SUB-01 | CreatePurchase stock-in | **PASS** | HTTP 200 `success=true` |
| PO-SUB-02 | Listado movimientos Purchase | **PASS** | HTTP 200 |
| PO-SUB-03 | SupplierAnalysis | **BLOCKED** | Stub vacío |
| PO-SUB-04 | StockTransfer Index | **PASS** | Flujo approve cercano; Reject no cableado |

---

## Validaciones pedidas vs realidad

| Área | Estado |
|------|--------|
| Inventario vía OC | No — solo movimiento `Purchase` manual |
| Costos pactados / diferencias de precio | No |
| Auditoría de OC | No |
| Reportes de compras | Stub |
| Estados OC | No |
| Permisos de compras | Solo `InventoryAccess` para stock-in |

---

## Conclusión

**Órdenes de Compra NO está listo para producción.** Es un gap estructural (módulo no construido), no un defecto puntual corregible en certificación.
