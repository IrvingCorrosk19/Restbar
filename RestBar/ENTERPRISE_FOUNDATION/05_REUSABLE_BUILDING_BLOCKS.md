# 05 — REUSABLE BUILDING BLOCKS

**Regla de oro:** Extender estos bloques. No crear paralelos.

---

| Block | Path | Extiende a | Cómo (sin romper) |
|-------|------|------------|-------------------|
| **Shift** | `EnterpriseOperations.Shift` + `ShiftController` | **Caja** | `CashSession.ShiftId` FK opcional; Shift = persona/tiempo; Cash = dinero |
| **Invoice** | `Invoice` + `InvoiceService` | **Precuenta / Fiscal** | Agregar InvoiceType (PreBill, Fiscal); controller nuevo |
| **Recipe / RecipeLine** | + `RecipeController` + InventoryOps | **Food Cost** | Cost rollup desde Product.Cost / último PO; UI después |
| **InventoryMovement** | + MovementType enum | **Compras / Merma / COGS** | Tipos ya incluyen Purchase/Waste/Sale; agregar UnitCost + refs |
| **InventoryOperationsService** | ~174 LOC cohesivo | **PO receive, waste, FC** | Métodos `ReceivePurchaseAsync`, `RecordWasteAsync` |
| **Customer** + LoyaltyPoints | `CustomerService` | **CRM / Loyalty** | Txn table después; capturar en Order |
| **DiscountPolicy** | + AdvancedSettings | **Promos** | Reglas stack/target; no nueva tabla precio base |
| **PriceScheduleService** | Aplica ventanas DiscountPolicy | **Happy Hour** | Completar UI; renombrar mentalmente PricingEngine |
| **AdvancedReportsService** | Queries reales | **Dashboard / BI seed** | Extraer `ICommandCenterSnapshot`; no clonar queries |
| **SalesReportService** | Filtros sólidos | **CC widgets** | Reusar |
| **TenantSubscriptionMiddleware** | Suspende writes | **SaaS Billing** | Entidad Subscription después; middleware ya gatea |
| **OrderHub** + OrderHubService | Groups station/table/order | **Alertas CC** | Groups `branch_{id}`, `cash_{id}` |
| **NotificationService** | Persist notifications | **Alert engine** | Tipos Severity + deep links |
| **AuditLog / AuditMiddleware** | Request audit | **Cash/PO audit** | Module field ya admite "Supplier" |
| **PaymentRefund / TipAllocation** | Models | **Cierre caja tips** | Liquidar en CashSession |
| **ProductPreparationStep** | Routing | Mantener excelencia KDS | No tocar en foundation |
| **CommissionRule** | Model | Nómina light | UI después |

---

# Anti-patterns (prohibidos)

| Prohibido | Por qué |
|-----------|---------|
| Nuevo "Order2" / "Kitchen2" | Duplicación |
| Stock ledger paralelo a InventoryMovement | Doble verdad |
| Promo engine que ignore DiscountPolicy | Dos precios |
| Food cost sin Recipe | BOM divergente |
| Caja sin relación a Shift/User | Auditoría imposible |
| BI que escriba en tablas Order | Contaminación OLTP |

---

# Mapa de extensión visual

```
Shift ──────────────► CashSession ──► Z-Report
Invoice ────────────► PreBill / FiscalAdapter
Recipe + Movement ──► FoodCost + Variance
Movement.Purchase ──► PO + Supplier + Receipt
DiscountPolicy ─────► HH / Promo
Customer ───────────► LoyaltyTxn + CRM
AdvancedReports ────► CommandCenter + bi_*
TenantMiddleware ───► Subscription/Plan
OrderHub ───────────► Alert fanout
Notification ───────► Copilot action inbox
```
