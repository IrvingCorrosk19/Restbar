# 07 — Trazabilidad de lotes

**Fecha:** 2026-07-30

---

## 1. Qué existe

| Campo / entidad | Evidencia |
|-----------------|-----------|
| `GoodsReceiptLine.LotNumber` | Solo en línea de recepción de mercancía |
| `GoodsReceiptLine.ExpiryDate` | Solo en línea de recepción |

No hay maestro de lotes, ni entidad Lot, ni FEFO.

---

## 2. Qué no existe

| Capacidad | Estado |
|-----------|--------|
| Lot master | NO IMPLEMENTADA |
| FEFO (first expiry first out) | NO IMPLEMENTADA |
| Asignación de lote en Sale / Deduct POS | No evidenciada |
| Lote en transferencias | No evidenciada |
| Lote en ajustes / merma | No evidenciada |
| Valuación / costo por lote | NO IMPLEMENTADA |
| Kardex por lote | No — kardex es `InventoryMovement` sin maestro de lotes |

---

## 3. Alcance real de trazabilidad

```
Recepción (GoodsReceiptLine)
  → puede capturar LotNumber + ExpiryDate
  → no continúa a inventario por lote / consumo / FEFO
```

La trazabilidad termina en el documento de recepción. Stock posterior vive en `Product.Stock` / `ProductStockAssignment` **sin dimensión de lote**.

---

## 4. Relación con gaps

| Gap | Prioridad (gap analysis) |
|-----|--------------------------|
| Maestro de lotes + FEFO | P1 |
| Extender lote más allá de recepción | Implícito en P1 (hoy PARTIAL solo en GoodsReceiptLine) |
