# 01 — Product Inventory

**RB-999 Final Release Candidate Audit** · **Fecha:** 2026-07-31  
**Regla:** sin nuevas funcionalidades — solo inventario objetivo.

## Superficie

| Activo | Cantidad (evidencia) |
|--------|---------------------:|
| Controllers | ~44–47 |
| Views folders | 38 |
| Service `.cs` files | ~63+ |
| Unit test methods (suite) | **95 PASS** (2026-07-31) |
| Playwright spec files | ~34 |
| Browser module folders | ~25 |
| Feature flags | 12 |

## Módulos / dominios

| Dominio | Controllers / UI | Flag |
|---------|------------------|------|
| Auth / RBAC | Auth, User*, Permission middleware | — |
| POS / Orders | Order, Table, Area, Station | — |
| KDS / Kitchen | KitchenApi, Station | — |
| Payments | Payment, PaymentView, Split | — |
| Shifts | Shift | — |
| Inventory | Inventory, Movement, StockTransfer, PSA | — |
| Recipes | Recipe | — |
| Cash | CashSession/Register/Movement/Report | `EnableCashModule` |
| Procurement | Supplier, PurchaseOrder, ProcurementDashboard | `EnablePurchasingModule` |
| Food Cost | FoodCostDashboard | `EnableFoodCostModule` |
| BI Nativo | BiNative, AnalyticsApi | (policy Analytics*) |
| Executive Analytics / CC | ExecutiveAnalytics, CommandCenter | `EnableCommandCenter` |
| Decision Intelligence | DecisionIntelligence (+ API) | `EnableDecisionIntelligence` |
| Business Rules | BusinessRules (+ API) | `EnableBusinessRules` |
| Copilot | Copilot | `EnableCopilot` (**false**) |
| Reports | Reports, AdvancedReports | `EnableReportExports` |
| Admin / MT | Company, Branch, SuperAdmin, AdvancedSettings | — |
| Audit | Audit | — |

## Feature Flags (Production)

ON: Cash, Purchasing, FoodCost, CommandCenter, ReportExports, DecisionIntelligence, BusinessRules  
OFF: Copilot, SeedEndpoints  

## Integraciones reales

| Tipo | Estado |
|------|--------|
| PostgreSQL | Sí |
| SignalR OrderHub | Sí |
| Email (MailKit) | Sí (advisories moderate) |
| Payment gateway / PCI processor | **No** |
| Offline sync | **No** |
| Weather / holiday APIs | **No** |
| Billing / SaaS metering UI | **No** |

## Certificaciones de programa existentes

RB-010 Cash · RB-020 Procurement · RB-023 Food Cost · RB-024 Inventory · RB-025 BI · RB-026 Production · RB-027 Quality Gate · RB-028 DI (`DECISION_INTELLIGENCE/`) · RB-029 Rules (`BUSINESS_RULES_ENGINE/`) · FULL_BROWSER · este RB-999.
