# 03 — Análisis de gaps

**Fecha:** 2026-07-30  
**Prioridad:** P0 = bloqueante operativo enterprise | P1 = alto valor | P2 = madurez / nice-to-have

---

## 1. Gaps priorizados

### P0

| Gap | Evidencia |
|-----|-----------|
| Conteos físicos | NO IMPLEMENTADA |
| Transferencias sin dispatch/receive | Solo Request / Approve / Reject; stock no tiene flujo de envío/recepción formal |
| Multibodega formal | No hay Warehouse; Station actúa como ubicación — riesgo de ambigüedad roles cocina/barra vs bodega |

### P1

| Gap | Evidencia |
|-----|-----------|
| Maestro de lotes / FEFO | Solo LotNumber/ExpiryDate en `GoodsReceiptLine` |
| Identificadores producto (SKU/Barcode) | Ausentes en Product |
| Unidades alternativas | Solo `Unit` string |
| RefundRestore sin uso | Tipo existe; no integrado |
| Vínculo Cash ↔ inventario | Sin link |
| Costo por ubicación / métodos avanzados | Solo WAC global en recepción; no Standard/FIFO/LIFO |

### P2

| Gap | Evidencia |
|-----|-----------|
| Subcategorías de producto | No existen |
| Ampliar BI más allá de LowStock read-only | LowStock solo lectura |
| Modelo Warehouse explícito | Recomendado mapear Station→rol warehouse sin entidad nueva *aún* |

---

## 2. Corregido en ciclo RB-024

| Ítem | Detalle |
|------|---------|
| Deducción receta POS | Ahora aplica waste + yield vía `InventoryOperationsService.ComputeRecipeIngredientQty` |
| Rechazo de transferencia | Nuevo `StockTransfer/Reject` |
| Snapshot enterprise | Nuevo `Inventory/GetEnterpriseSnapshot` |

---

## 3. Fuera de alcance / no inventado

No se documentan estimaciones de esfuerzo ni diseños de UI no evidenciados en el audit.
