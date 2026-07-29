# 06 — MODULE BLUEPRINT

**Regla:** justificar cada módulo con la pregunta de valor.  
**Regla:** extender building blocks; no duplicar.

---

# A. Módulos a CONSTRUIR / COMPLETAR (prioridad)

## 1. Cash Management (Caja) — BUILD (extiende Shift)

| Campo | Valor |
|-------|-------|
| ¿Gana/ahorra/controla/decide? | **Ahorra + controla** |
| Justificación | Sin caja no hay operación continua ni anti-fraude |
| Entidades | `CashRegister`, `CashSession` (ligada a `Shift`), `CashMovement` |
| No duplicar | No crear “otro turno”; Shift = contenedor laboral, CashSession = dinero |
| Depende de | Shift existente |
| Prioridad | **P0** |

## 2. Pre-Account & Print — BUILD (extiende Order/Invoice)

| ¿Valor? | **Gana** (rotación) + CX |
| Entidades | PreBill snapshot; print jobs |
| Prioridad | **P0** |

## 3. Fiscal Invoicing — COMPLETE (`InvoiceService`)

| ¿Valor? | **Legal + comercial** |
| Enfoque | Adapter por país (DI); empezar 1 jurisdicción piloto |
| Prioridad | **P0** (paralelo post-caja) |

## 4. Suppliers & Purchase Orders — BUILD

| ¿Valor? | **Ahorra** (food cost) |
| Entidades | `Supplier`, `PurchaseOrder`, `POLine`, `GoodsReceipt` |
| Extiende | `InventoryMovement.Purchase` al recibir |
| Reusar | JS `supplier-management.js` + vistas SupplierAnalysis |
| Prioridad | **P0** |

## 5. Recipe Costing & Food Cost — COMPLETE UI + cost engine

| ¿Valor? | **Ahorra + decide** |
| Extiende | `Recipe`, `RecipeLine`, cost from last PO |
| KPIs | Theoretical FC%, Actual FC%, variance |
| Prioridad | **P0–P1** |

## 6. Waste / Merma — BUILD ligero

| ¿Valor? | **Ahorra** |
| Extiende | `InventoryMovementType.Waste` + motivo + costo |
| Prioridad | **P1** |

## 7. Promo Engine (HH / Discount rules) — COMPLETE

| ¿Valor? | **Gana** |
| Extiende | `DiscountPolicy`, `PriceScheduleService` |
| Prioridad | **P1** |

## 8. Combos / Bundles — BUILD

| ¿Valor? | **Gana** (ticket) |
| Entidades | `Combo`, `ComboItem` → OrderItem expansion |
| Prioridad | **P1** |

## 9. Executive Command Center — BUILD

| ¿Valor? | **Decide** |
| Reusa | AdvancedReportsService + OrderHub alerts |
| Prioridad | **P1** |

## 10. CRM & Loyalty — COMPLETE

| ¿Valor? | **Gana** (recompra) |
| Extiende | `Customer`, LoyaltyPoints + captura POS |
| Prioridad | **P2** |

## 11. Forecast & Reorder — BUILD

| ¿Valor? | **Automatiza + ahorra** |
| Requiere | Historial ventas + stock + recetas |
| Prioridad | **P2** |

## 12. Labor Scheduling — BUILD / Integrate

| ¿Valor? | **Ahorra** (prime cost) |
| Enfoque | MVP schedule + hours; payroll vía integración |
| Prioridad | **P2** |

## 13. Delivery Hub — INTEGRATE

| ¿Valor? | **Gana** |
| Enfoque | Conectores, no marketplace propio |
| Prioridad | **P2–P3** |

## 14. Reservations — BUILD ligero o integrar

| ¿Valor? | **Gana** (ocupación) |
| Prioridad | **P3** (salvo ICP lo exija) |

## 15. Franchise Pack — BUILD

| ¿Valor? | **Escala** |
| Benchmark, brand menu lock, royalty report |
| Prioridad | **P2–P3** |

## 16. BI Platform — DESIGN→BUILD

| ¿Valor? | **Decide** |
| Warehouse + KPI store (ver doc 08) |
| Prioridad | **P1 diseño / P2 build** |

## 17. AI Copilot — DESIGN→BUILD

| ¿Valor? | **Decide + automatiza** |
| Sobre BI + reglas |
| Prioridad | **P3** (tras datos limpios) |

## 18. SaaS Billing & Onboarding — BUILD

| ¿Valor? | **Comercial RestBar** |
| Extiende | TenantSubscriptionMiddleware |
| Prioridad | **P2** |

---

# B. Módulos a NO construir (24 meses)

| Módulo | Motivo |
|--------|--------|
| Payroll nativo | Integrar |
| Hotel PMS / casino chips | Fuera ICP |
| App store completa | Prematuro |
| ERP contable completo | Export + integración |
| Producción fábrica industrial | Solo Central Kitchen light si cadena lo pide |

---

# C. Módulos a REDISEÑAR (no reescribir)

| Actual | Rediseño |
|--------|----------|
| AdvancedReports | Alimentan Command Center; unificar con Reports |
| Inventory | Ledger único: venta, PO, merma, transfer |
| Home/Dashboard | Reemplazar por Command Center |
| AdvancedSettings | Completar vistas o un solo settings hub |
| Payment entry points | Un solo menú PaymentView |

---

# D. Módulos a ELIMINAR / OCULTAR ahora

| Superficie | Acción |
|------------|--------|
| SupplierAnalysis con ceros | Ocultar hasta G04 |
| Export vacío | Deshabilitar botón o implementar |
| AdvancedSettings rotas | Quitar links |
| Seed prod | Bloquear |
| Menú Payment huérfano | Redirect |

---

# E. Mapa de dependencias

```
Shift ──► CashSession ──► Z-Report ──► Fiscal
Recipe ──► FoodCost ◄── PO Receipt ◄── Supplier
PriceSchedule ──► HH ──► Combo (opcional paralelo)
Reports APIs ──► Command Center ──► BI WH ──► Copilot
Customer ──► Loyalty ──► CRM campaigns
```
