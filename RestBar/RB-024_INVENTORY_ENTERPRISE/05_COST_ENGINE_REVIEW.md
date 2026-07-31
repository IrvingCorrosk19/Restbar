# 05 — Revisión del motor de costos

**Fecha:** 2026-07-30

---

## 1. Qué existe

| Componente | Evidencia |
|------------|-----------|
| Campos de costo en Product | `Cost`, `AverageCost`, `LastPurchaseCost` |
| WAC en recepción | `ProcurementCostEngine` |
| Historial de precios | `PriceHistory` |
| FoodCost | Waste + costing (integración activa) |
| Deducción receta | Qty con waste+yield (`ComputeRecipeIngredientQty`) alimenta consumo |

---

## 2. Qué no existe

| Método / capacidad | Estado |
|--------------------|--------|
| Costo estándar (Standard) | NO IMPLEMENTADA |
| FIFO | NO IMPLEMENTADA |
| LIFO | NO IMPLEMENTADA |
| Costo por ubicación / bodega | NO IMPLEMENTADA |
| Costo por lote | NO IMPLEMENTADA (solo LotNumber/Expiry en recepción) |

---

## 3. Flujo evidenciado

```
Procurement Complete
  → Restore stock
  → Log InventoryMovement Purchase
  → ProcurementCostEngine actualiza WAC / costos producto
  → PriceHistory (historial)
```

POS / recetas consumen stock; FoodCost usa merma y costing. Cash **no** enlaza al motor de inventario/costo.

---

## 4. Conclusión (evidencia)

El motor de costo operativo es **WAC en recepción** más campos de costo en producto e historial de precios. No hay evidencia de valuación Standard, FIFO, LIFO ni costo multi-ubicación.
