# 01 — BUSINESS ANALYSIS

**Fecha:** 2026-07-29  
**Módulo:** RB-020 Procurement Enterprise

---

# 1. Estado actual RestBar

| Capacidad | Estado | Evidencia |
|-----------|--------|-----------|
| Product + Stock + MinStock | ✅ | `Product.cs` |
| InventoryMovement (Purchase/Adjustment/Waste/Sale) | ✅ parcial | `CreatePurchase` = stock bump ad-hoc, sin proveedor/costo |
| Recipe explosion en venta | ✅ | `InventoryOperationsService` |
| Recipe UI / costeo teórico | ❌ | API JSON only, sin UnitCost en RecipeLine |
| StockTransfer estación↔estación | ✅ | |
| Supplier entity | ❌ | Solo JS huérfano + SupplierAnalysis ceros |
| PurchaseRequest / PO / Receipt | ❌ | |
| Product.Cost automático | ❌ | Manual en Product CRUD |
| Warehouse | ❌ | Station = ubicación (correcto para F&B) |
| EnablePurchasingModule | ⚠️ declarado, no usado | |
| PurchasingAccess policy | ⚠️ declarada, no usada | |

**Conclusión:** RestBar controla stock operativo pero **no controla gasto de compras**. El food cost reportado es estático e incorrecto tras cualquier recepción.

---

# 2. Dolor del negocio (LATAM F&B)

1. Compras por WhatsApp / teléfono → sin trazabilidad  
2. Precios suben sin alerta → margen se erode  
3. Recepción sin conteo → faltantes absorbidos como “merma”  
4. Mismo producto, 3 proveedores, nadie sabe cuál conviene  
5. Fraud: pedidos fantasma, precios inflados, devoluciones no registradas  
6. Food cost % inventado o calculado semanal a mano  

---

# 3. Benchmarks (conceptos, no UI)

| Concepto | McD / QSR | R365 / MarketMan | RestBar RB-020 |
|----------|-----------|------------------|----------------|
| Par-driven reorder | ✅ | ✅ | ✅ Command Center |
| Approved supplier catalog | ✅ | ✅ | ✅ SupplierProduct |
| PO obligatorio (no emergencia) | ✅ | ✅ | ✅ + Emergency override auditado |
| Goods Received Note | ✅ | ✅ | ✅ GoodsReceipt |
| Three-way match (PO·GRN·Invoice) | ✅ | ✅ | ✅ diseño; Invoice AP fase 2 |
| Weighted average cost | ✅ | ✅ | ✅ Cost Engine |
| Theoretical food cost | ✅ | ✅ | ✅ Recipe × Cost |
| Supplier scorecard | ⚠️ | Básico | ✅ automático |
| Multitenant LATAM (Yappy/multiempresa) | ❌ | ⚠️ | ✅ nativo |

**Diferenciadores RestBar:** multitenancy Company→Branch nativo, integración POS+KDS+Inventory en un solo stack, hash forense (patrón RB-010), precio LATAM-friendly.

---

# 4. Ciclo de valor

```
Necesidad (par/min stock / forecast)
  → Purchase Request
  → Aprobación
  → Cotización / Preferred Supplier
  → Purchase Order
  → Envío proveedor
  → Goods Receipt (± parcial / rechazo / backorder)
  → InventoryMovement.Purchase + Cost Engine
  → Recipe theoretical cost refresh
  → Food Cost % / Dashboard / BI / Copilot inputs
  → Supplier Score update
  → Auditoría forense
```

---

# 5. Qué NO construir

- Segundo inventario paralelo  
- Warehouse genérico (usar Station)  
- Supplier duplicado (hoy no existe → crear UNO)  
- Invoice de ventas como AP (Invoice actual = sales)  
- OCR invoice en v1 (fase 2)  
- Marketplace de proveedores  

---

# 6. ROI esperado

| Métrica | Impacto típico industria | Target RestBar año 1 |
|---------|--------------------------|----------------------|
| Food cost variance | −1 a −3 pts | −1.5 pts |
| Overpay detection | 0.5–2% spend | ≥0.8% |
| Time to receive | −50% | <2 min/línea |
| Fraud incidents | ↓ visible | 100% auditados |

---

# 7. Dependencias

- **Requiere intactos:** Product, InventoryMovement, Recipe, Station, RB-010 Cash  
- **Alimenta:** RB-023/024 food cost, RB-050 Command Center, RB-070 BI, RB-080 Copilot  
