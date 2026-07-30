# 11 — SUPPLIER SCORE

---

# Dimensiones (0–100)

| Dimensión | Peso v1 | Señal |
|-----------|---------|-------|
| Price | 25% | vs promedio catálogo / histórico |
| OTIF | 30% | on-time & in-full receipts |
| Quality | 25% | rejected+damaged / received |
| Reliability | 20% | cancelaciones, disputes, lead variance |

```
Overall = 0.25*Price + 0.30*OTIF + 0.25*Quality + 0.20*Reliability
```

---

# Cálculo OTIF

```
OnTime = received_at <= expected_delivery + grace(1 day)
InFull = qty_accepted >= qty_ordered * 0.98
OTIF% = count(OnTime AND InFull) / count(completed receipts) * 100
```

---

# Triggers recompute

- GoodsReceipt.Completed  
- PO.Cancelled after Sent  
- Nightly job (v1.1)  
- Manual admin recompute  

Cached en `Supplier.ScoreOverall` + snapshot `SupplierScore`.

---

# Automatización recomendaciones

| Necesidad | Regla |
|-----------|-------|
| Más barato | min agreed_unit_price active |
| Más rápido | min lead_time_days |
| Más confiable | max Overall donde OTIF≥80 |
| Recomendado | Preferred OR max Overall |
| Riesgo | Blacklisted / score < 40 → alerta |
