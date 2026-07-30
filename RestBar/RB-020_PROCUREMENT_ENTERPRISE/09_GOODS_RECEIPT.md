# 09 — GOODS RECEIPT

---

# Propósito

Documento de verdad de **qué entró** al restaurante.  
Dispara inventario + costo + score + auditoría.

---

# Estados

Draft → InProgress → Completed | Cancelled | Disputed

---

# Flujo recepción

1. Seleccionar PO Sent/Partial  
2. Wizard línea por línea: qty_received, disposition, lot, expiry, temp OK  
3. Supervisor opcional si variance o daño  
4. Complete → inventariar solo `qty_accepted`  
5. Actualizar `POLine.quantity_received`  
6. Recalcular estado PO (Partial / Full)  

---

# Disposition

| Disposition | Stock | Costo | Score impacto |
|-------------|-------|-------|---------------|
| Accepted | +qty_accepted | sí | OTIF + |
| Partial | +accepted | sí | OTIF − |
| Short | 0 del faltante | no | OTIF − |
| Damaged | 0 (o merma aparte) | no | Quality − |
| Rejected | 0 | no | Quality − |
| Over | +ordered max; exceso → ajuste auditado | sí hasta ordered | alerta |

---

# Backorder

Líneas Short/Partial dejan PO en PartiallyReceived.  
No auto-crea segundo PO (usuario decide).

---

# Inspección v1

Campos: temperature_ok, lot_number, expiry_date, notes.  
Fotos/firma: v1.1 (attachments table).

---

# Idempotencia

`Complete` es idempotente por `receipt_number` + status Completed (no re-inventariar).
