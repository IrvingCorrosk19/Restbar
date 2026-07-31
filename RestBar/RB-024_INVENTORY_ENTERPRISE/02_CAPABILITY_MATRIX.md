# 02 — Matriz de capacidades

**Fecha:** 2026-07-30  
**Leyenda:** EXISTS = implementado | PARTIAL = parcial | NO IMPLEMENTADA = no existe

---

| Capacidad | Estado | Evidencia |
|-----------|--------|-----------|
| Stock global (`Product.Stock`) | EXISTS | Modelo Product |
| Stock por ubicación (`ProductStockAssignment`) | EXISTS | Ubicación = Station |
| Entidad Warehouse | NO IMPLEMENTADA | — |
| Kardex / movimientos | EXISTS | `InventoryMovement` |
| Tipo Purchase | EXISTS | Log en Complete procurement |
| Tipo Adjustment | EXISTS | Tipo en enum/modelo |
| Tipo Waste | EXISTS | Integración FoodCost / merma |
| Tipo TransferOut / TransferIn | EXISTS | Transferencias estación↔estación |
| Tipo Sale | EXISTS | Deduct POS |
| Tipo CancelRestore | EXISTS | Tipo en modelo |
| Tipo RefundRestore | PARTIAL | Existe; **no usado** |
| TrackInventory | EXISTS | Product |
| Unidad de medida | PARTIAL | `Unit` string; sin alt. units |
| SKU | NO IMPLEMENTADA | — |
| Barcode | NO IMPLEMENTADA | — |
| Subcategorías producto | NO IMPLEMENTADA | — |
| Cost / AverageCost / LastPurchaseCost | EXISTS | Product |
| Recetas + líneas | EXISTS | Recipe / RecipeLine |
| YieldPercent / WastePercent | EXISTS | RecipeLine |
| Alternativas de ingrediente | EXISTS | IngredientAlternative |
| Deducción POS con waste+yield | EXISTS | `ComputeRecipeIngredientQty` (RB-024) |
| Transfer request | EXISTS | StockTransfer |
| Transfer approve | EXISTS | StockTransfer |
| Transfer reject | EXISTS | Nuevo — `StockTransfer/Reject` |
| Transfer dispatch | NO IMPLEMENTADA | — |
| Transfer receive | NO IMPLEMENTADA | — |
| Conteos físicos | NO IMPLEMENTADA | — |
| LotNumber / Expiry en recepción | PARTIAL | Solo `GoodsReceiptLine` |
| Maestro de lotes | NO IMPLEMENTADA | — |
| FEFO | NO IMPLEMENTADA | — |
| Costo WAC en recepción | EXISTS | `ProcurementCostEngine` |
| PriceHistory | EXISTS | — |
| Standard / FIFO / LIFO | NO IMPLEMENTADA | — |
| Costo por ubicación | NO IMPLEMENTADA | — |
| Integración Procurement → stock | EXISTS | Complete → Restore + Purchase |
| Integración POS → stock | EXISTS | SendToKitchen → Deduct |
| Integración FoodCost | EXISTS | Waste + costing |
| Integración Cash | NO IMPLEMENTADA | Sin vínculo |
| BI LowStock | EXISTS | Solo lectura |
| Snapshot enterprise | EXISTS | `Inventory/GetEnterpriseSnapshot` |
| Station como bodega (rol) | PARTIAL | Tipos Station (cocina/barra); sin entidad Warehouse |

---

**Totales (filas de capacidad):** EXISTS ~24 | PARTIAL ~5 | NO IMPLEMENTADA ~14
