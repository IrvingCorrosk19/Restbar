# 03 — DATABASE ANALYSIS

**Context:** `Models/RestBarContext.cs` · 42 DbSets · PostgreSQL

---

# 1. Fortalezas

- UUID PKs + `gen_random_uuid()`  
- Enums PG (`user_role_enum`)  
- Concurrency token en Order (migración existente)  
- FK indexes generados por migraciones en la mayoría de FKs  
- Enterprise ops ya en schema: Recipe, InventoryMovement, Shift, Refund, Tips  

---

# 2. Debilidades

| Issue | Impacto |
|-------|---------|
| Solo **3** índices fluent explícitos (email, legalId, stock assignment unique) | KDS/reportes lentos a escala |
| Sin índices compuestos status+branch+time | Full scans en cocina/abiertos |
| `ProductCategory` sin tenant | Legacy peligroso |
| CompanyId/BranchId **nullable** en ops | Datos huérfanos / filtros frágiles |
| Sin `HasQueryFilter` tenant | Depende de cada service |
| Sin tabla Supplier/PO | Purchase movement sin contraparte |
| Shift sin CashSession FK futuro | Hay que diseñar extensión 1:1/1:N |
| RecipeLine sin `UnitCost` snapshot | Food cost histórico difícil |
| InventoryMovement sin `UnitCost` / `SupplierId` | PO recepción incompleta |
| Sin schema `bi_*` | Analytics golpea OLTP |
| Secrets en `OnConfiguring` | Riesgo seguridad |

---

# 3. Preparación por módulo (campos — diseñar ahora, crear después)

## Cash (sobre Shift)
- `cash_registers` (BranchId, Name, IsActive)  
- `cash_sessions` (ShiftId?, RegisterId, OpeningFloat, ClosingCounted, Expected, Status)  
- `cash_movements` (SessionId, Type, Amount, Reason)  
**No crear en 0.5** — documentar contrato.

## Purchasing
- `suppliers` (CompanyId, tax id, terms)  
- `purchase_orders` / `po_lines`  
- `goods_receipts` → escribe `InventoryMovement` + UnitCost  

## Costing / Merma
- Extender Movement: UnitCost, WasteReasonId  
- `recipe_cost_snapshots` opcionales  

## BI / Forecast
- Schema `bi` facts: sales_line_daily, inventory_daily  
- Jobs nocturnos — no vistas materializadas aún obligatorias  

---

# 4. Índices recomendados (aplicar en foundation si seguro)

```sql
-- Orders ops
CREATE INDEX IX_orders_branch_status_opened ON orders (branch_id, status, opened_at);
CREATE INDEX IX_orders_table_status ON orders (table_id, status);

-- Order items kitchen
CREATE INDEX IX_order_items_order_status ON order_items (order_id, status);
CREATE INDEX IX_order_items_station_status ON order_items (station_id, status);

-- Payments
CREATE INDEX IX_payments_branch_paid_at ON payments (branch_id, paid_at);

-- Inventory ledger
CREATE INDEX IX_inv_mov_product_created ON inventory_movements (product_id, created_at);
CREATE INDEX IX_inv_mov_company_created ON inventory_movements (company_id, created_at);

-- Shifts
CREATE INDEX IX_shifts_user_active ON shifts (user_id, is_active);

-- Discount policies
CREATE INDEX IX_discount_policies_company_active ON discount_policies (company_id, is_active);
```

---

# 5. Normalización

- Category vs ProductCategory → **deprecar ProductCategory** (fase cleanup)  
- Persons vs Customers → roles distintos (OK mantener)  
- Payments + SplitPayments → OK  

---

# 6. Escalabilidad DB

| Escala | Estrategia |
|--------|------------|
| 1–20 branches | Índices + AsNoTracking lecturas |
| 20–100 | Particionar reportes a `bi_*`; read replica opcional |
| 100+ | Warehouse externo; no partir tenants en DB distintas aún |

**Regla:** No multi-DB por tenant en 24 meses.
