# 01 — COMPLETE MODULE INVENTORY

**Base:** `RestBar/` · Discovery 2026-08-01 · 49 controllers · sin Areas MVC

## Controllers → dominio

| Controller | Dominio | UI Views | API-only | Flag |
|------------|---------|----------|----------|------|
| AuthController | Auth | Auth/* | | |
| HomeController | Shell | Home | | |
| SuperAdminController | Tenancy | SuperAdmin/* | | Roles=superadmin |
| CompanyController | Admin | Company | | |
| BranchController | Admin | Branch | | |
| AreaController | Floors | Area | | |
| TableController | Tables | Table | | |
| StationController | Stations | Station | | |
| CategoryController | Catalog | Category | | |
| ProductController | Catalog | Product | stock JSON | |
| ModifierController | Catalog | Modifier | | |
| CustomerController | CRM | Customer | | |
| UserController / UserManagement / UserAssignment | RBAC | User* | | UserManagement |
| OrderController | POS/KDS | Order | many JSON | |
| KitchenApiController | KDS | — | yes | |
| PaymentController | Payments | — | yes | |
| PaymentViewController | Payments | PaymentView | | |
| PersonController | Split | — | yes | |
| CashSession / CashRegister / CashMovement / CashReport | Cash | Cash* (Movement JSON) | Movement | EnableCashModule |
| Inventory / InventoryMovement / StockTransfer / ProductStockAssignment | Inventory | Inventory, Assignments | Movement, Transfer | none |
| Supplier / PurchaseOrder / ProcurementDashboard | Procurement | * | | EnablePurchasingModule |
| FoodCostDashboard / Recipe | FoodCost | * | | EnableFoodCostModule |
| Reports / AdvancedReports | Reports | * | | |
| ExecutiveAnalytics / BiNative / ExecutiveCommandCenter | Analytics | * | | EnableCommandCenter |
| DecisionIntelligence / BusinessRules / Copilot | Intelligence | * | | DI/BR/Copilot |
| Audit / Email / AdvancedSettings / Shift / Seed | Platform | * | Seed Dev-only | SeedEndpoints |

## Services (carpetas)

`Services/Analytics`, `BusinessRules`, `Cash`, `Copilot`, `DecisionIntelligence`, `FoodCost`, `Intelligence`, `Procurement` + root Order/Auth/Inventory/Product/Payment/…

## Feature Flags (`Infrastructure/Foundation/FeatureFlags.cs`)

EnableCashModule, EnablePurchasingModule, EnableFoodCostModule, EnableCommandCenter, EnableCopilot, EnableDecisionIntelligence, EnableBusinessRules, EnableSeedEndpoints, EnableReportExports, EnableSupplierUi, EnableBackupExecution, EnableAdvancedSettingsExtra

## NOT IMPLEMENTED / parcial (no marcar PASS UI)

| Capacidad | Estado real |
|-----------|-------------|
| Unión de mesas formal | NOT IMPLEMENTED (verificar UI) |
| StockTransfer UI rica | PARCIAL (JSON Index) |
| CashMovement UI | PARCIAL (API) |
| Copilot en Production | DESHABILITADO (flag false) |
| Seed HTTP en Production | BLOQUEADO (EnableSeedEndpoints false + Dev gate) |
| Payment gateway PCI externo | NOT IMPLEMENTED |
| Offline POS | IMPLEMENTADO (SW + IndexedDB queue) — revalidar E2E |

## Roles (`UserRole`)

superadmin, admin, manager, supervisor, waiter, cashier, chef, bartender, accountant, support, inventarista
