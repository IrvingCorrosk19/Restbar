# 04 — Análisis de kardex

**Fecha:** 2026-07-30  
**Modelo:** `InventoryMovement`

---

## 1. Tipos de movimiento

| Tipo | Uso evidenciado |
|------|-----------------|
| Purchase | Log al completar procurement / recepción |
| Adjustment | Tipo existe |
| Waste | Integración merma / FoodCost |
| TransferOut | Transferencia estación origen |
| TransferIn | Transferencia estación destino |
| Sale | Deducción POS (SendToKitchen → Deduct) |
| CancelRestore | Tipo existe (restauración por cancelación) |
| RefundRestore | Tipo existe; **unused** |

---

## 2. Orígenes que escriben kardex (evidencia)

| Origen | Acción | Tipo(s) |
|--------|--------|---------|
| Procurement Complete | Restore + log | Purchase |
| POS SendToKitchen | Deduct | Sale |
| FoodCost / Waste | Registro merma | Waste |
| StockTransfer | Movimientos entre estaciones | TransferOut / TransferIn |

---

## 3. Observaciones (solo hechos)

- El kardex cubre compra, venta, merma, ajuste y transferencias.
- `RefundRestore` está modelado pero no hay uso evidenciado.
- No hay evidencia de kardex ligado a Cash.
- Conteos físicos no generan movimientos (capacidad no implementada).
- Lotes no participan en el kardex más allá de datos en línea de recepción.

---

## 4. Relación con stock

| Capa | Rol |
|------|-----|
| `Product.Stock` | Stock global |
| `ProductStockAssignment` | Stock por Station |
| `InventoryMovement` | Auditoría / kardex |

No existe entidad Warehouse; las ubicaciones del kardex de transferencia son estaciones.
