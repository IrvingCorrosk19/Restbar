# 02 — DOMAIN ANALYSIS

---

# 1. Dominios actuales (de facto)

| Dominio | Entidades / Services | Madurez |
|---------|----------------------|---------|
| Tenancy | Company, Branch, SuperAdmin, TenantSubscriptionMiddleware | Alta |
| Identity | User, Auth, Roles, Policies | Media-Alta |
| Catalog | Product, Category, Station, StockAssignment | Alta |
| Floor | Area, Table, merge/split | Media |
| Order / POS | Order, OrderItem, Person, OrderService | Alta ops / Baja clean arch |
| Kitchen | StationOrders, KitchenService, PrepSteps, OrderHub | Alta |
| Payments | Payment, SplitPayment, Refund, PaymentService | Alta |
| Inventory | Movement, Transfer, InventoryOps, Recipe | Media |
| Shift | Shift, Handoff | Baja (API only) |
| Pricing | DiscountPolicy, PriceScheduleService | Media-Baja |
| Customer | Customer (+ LoyaltyPoints) | Baja |
| Invoice | Invoice + InvoiceService | Huérfano |
| Reporting | SalesReport, AdvancedReports | Media + stubs |
| Settings | System, Tax, Currency, Hours, Backup, Email | Media |
| Audit | AuditLog, GlobalLogging, AuditMiddleware | Media |

---

# 2. Dominios futuros → anclaje

| Futuro | Anclar a | Nuevas entidades (después) |
|--------|----------|----------------------------|
| **Cash** | Shift + Payment(cash) | CashRegister, CashSession, CashMovement |
| **Purchasing** | InventoryMovement.Purchase | Supplier, PurchaseOrder, GoodsReceipt |
| **Food Cost** | Recipe + Movement + Product.Cost | CostSnapshot, VariancePeriod |
| **Merma** | MovementType.Waste | WasteReason catalog |
| **Fiscal** | Invoice | FiscalDocument, TaxAdapter |
| **Combos** | OrderItem expansion | Combo, ComboItem |
| **Happy Hour** | DiscountPolicy + PriceSchedule | (extender, no duplicar) |
| **CRM/Loyalty** | Customer | Visit, LoyaltyTxn, Segment |
| **Dashboard/BI** | AdvancedReports → extract | bi_* facts/dims |
| **Copilot** | BI + Notifications | RecommendationLog |
| **SaaS Billing** | TenantSubscriptionMiddleware | Subscription, Plan, InvoiceSaaS |
| **Franquicias** | Company hierarchy | FranchiseAgreement, Royalty |

---

# 3. Reglas de dominio (contratos a respetar 5 años)

1. **Todo hecho económico** deja traza en ledger (InventoryMovement / futuro CashMovement / AuditLog).  
2. **CompanyId/BranchId** obligatorios en entidades operativas nuevas (no nullable en módulos nuevos).  
3. **Order** es aggregate de servicio; Cash/PO **no** viven dentro de OrderService.  
4. **Recipe** es fuente de verdad de BOM; Food Cost lee Recipe + costos, no inventa BOM paralelo.  
5. **DiscountPolicy** es motor de precio temporal; Combos son aggregate distinto que **expande** líneas.  
6. **IsActive** = desactivación lógica actual; soft-delete formal es fase posterior (no mezclar ya).

---

# 4. Bounded contexts recomendados (lógicos)

```
Tenancy | Identity | Catalog | FloorOps | OrderTaking | KitchenProduction
Payments | CashControl | Inventory | Purchasing | Costing
PricingPromos | CRM | Fiscal | ReportingAnalytics | PlatformBilling
```

Comunicación: eventos de dominio **in-process** primero (interfaces), outbox después si escala.
