# 01 — Auditoría de inventario (evidencia)

**Fecha:** 2026-07-30  
**Alcance:** RB-024 Inventory Enterprise — solo hechos verificados  
**Fuente:** auditoría de código / dominio RestBar

---

## 1. Qué existe

| Área | Evidencia |
|------|-----------|
| Stock | `Product.Stock` + `ProductStockAssignment` (ubicación = `Station`). No hay entidad `Warehouse`. |
| Kardex | `InventoryMovement` con tipos: Purchase, Adjustment, Waste, TransferOut, TransferIn, Sale, CancelRestore, RefundRestore |
| Producto | `TrackInventory`, `Unit` (string), `TaxRate`, `ImageUrl`, `IsActive`, `Cost` / `AverageCost` / `LastPurchaseCost` |
| Recetas | `Recipe` / `RecipeLine` con `YieldPercent`, `WastePercent`; existe `IngredientAlternative` |
| Transferencias | Solicitud / Aprobar / Rechazar; estación ↔ estación |
| Lotes (parcial) | Solo `GoodsReceiptLine.LotNumber` / `ExpiryDate` |
| Costo | WAC en recepción (`ProcurementCostEngine`); `PriceHistory` |
| Integraciones | Procurement Complete → Restore + Log Purchase; POS SendToKitchen → Deduct; FoodCost (merma + costing); BI LowStock (solo lectura) |
| Endpoints nuevos | `Inventory/GetEnterpriseSnapshot`, `StockTransfer/Reject` |

---

## 2. Qué funciona

| Flujo | Estado |
|-------|--------|
| Deducción POS con waste + yield | Sí — `InventoryOperationsService.ComputeRecipeIngredientQty` (fix RB-024) |
| Entrada por compra / recepción | Sí — Complete → Restore + movimiento Purchase |
| Transferencia request/approve/reject | Sí (rechazo nuevo en RB-024) |
| Snapshot enterprise | Sí — `GetEnterpriseSnapshot` |
| Alertas LowStock (BI / Inventory) | Sí — lectura |

---

## 3. Qué falta / no existe

| Capacidad | Evidencia |
|-----------|-----------|
| Entidad Warehouse | No |
| SKU / Barcode / Subcategorías / Unidades alternativas en Product | No |
| Despacho / recepción de transferencia | No |
| Conteos físicos | No implementado |
| Maestro de lotes / FEFO | No |
| Standard / FIFO / LIFO / costo por ubicación | No |
| RefundRestore en uso | Existe tipo; no usado |
| Vínculo Cash ↔ inventario | No |
| Multibodega formal | Solo vía tipos de `Station` (cocina/barra) |

---

## 4. Resumen ejecutivo

El inventario operativo se basa en **stock global + asignaciones por estación**, con kardex de movimientos y costo WAC en compras. Recetas ya aplican merma/rendimiento en POS. Faltan conteos, lotes maestros, despacho/recepción de transferencias y un modelo de bodega distinto de `Station`.
