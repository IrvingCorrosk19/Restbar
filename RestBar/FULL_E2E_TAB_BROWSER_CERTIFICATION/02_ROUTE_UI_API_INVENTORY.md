# 02 — ROUTE / UI / API INVENTORY

## Navegación principal (`Views/Shared/_Layout.cshtml`)

Company, Branch, Area, Table, Category, Station, Product, Modifier, Customer, AdvancedSettings, Email, BusinessRules, DecisionIntelligence/Cockpit, ExecutiveCommandCenter, ExecutiveAnalytics, BiNative, Copilot, Order, StationOrders (kitchen), Shift, CashSession/Dashboard, CashRegister, ProcurementDashboard, Supplier, PurchaseOrder, FoodCostDashboard, Recipe, User, Profile, Logout

## Rutas críticas sin ítem de menú (acceso por URL / Home cards)

| Ruta | Notas |
|------|-------|
| `/Inventory` | Inventario |
| `/ProductStockAssignment` | Asignaciones |
| `/PaymentView` | Dashboard pagos |
| `/Reports`, `/AdvancedReports` | Reportes |
| `/UserAssignment` | Asignaciones |
| `/Audit` | Auditoría |
| `/SuperAdmin` | Solo superadmin |
| `/CashReport/ZReport`, `XReport` | Reportes caja |
| `/Order/StationOrders?stationType=bar` | KDS Bar |

## ModuleDisabled (flag off)

BusinessRules, Supplier, CashSession, CashRegister, CashReport, ExecutiveCommandCenter, FoodCostDashboard, Copilot, DecisionIntelligence

## APIs JSON (sin View)

CashMovement, InventoryMovement, KitchenApi, Person, StockTransfer, AnalyticsApi, BusinessRulesApi, DecisionIntelligenceApi, PaymentController (`/api/Payment/*`)

## POS layout

`Views/Shared/_OrderLayout.cshtml` + `offline-pos.js` + `manifest.webmanifest` + `sw-restbar.js`

## Auth MFA

`/Auth/Login` → `/Auth/MfaChallenge` | `/Auth/MfaSetup` (privilegiados)
