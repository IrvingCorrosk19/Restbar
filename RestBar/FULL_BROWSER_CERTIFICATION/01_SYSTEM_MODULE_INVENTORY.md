# 01 — SYSTEM MODULE INVENTORY

**Fecha descubrimiento:** 2026-07-30  
**Commit:** `0ab2bd2`  
**Ambiente referencia:** VPS `http://164.68.99.83:8084` · Local `http://localhost:5001`  
**Fuente:** Controllers (43), Views (37 folders / 96 cshtml), FeatureFlags, Playwright `tests/Browser` (26 specs)

Estados UI: FUNCIONAL | PARCIAL | NO FUNCIONAL | NO ACCESIBLE | HUÉRFANO | DESHABILITADO | SOLO BACKEND | SOLO UI | NO DETERMINADO  
Pruebas: EXISTENTES Y EJECUTABLES | EXISTENTES PERO ROTAS | EXISTENTES INCOMPLETAS | NO EXISTEN

| ID | Módulo | Submódulo | Ruta | Controller/API | Roles / Policy | Feature Flag | Estado UI | Pruebas Browser | Criticidad |
|----|--------|-----------|------|----------------|----------------|--------------|-----------|-----------------|------------|
| M01 | Autenticación | Login/Logout | `/Auth/Login` | AuthController | AllowAnonymous / Authorize | — | FUNCIONAL | EXISTENTES Y EJECUTABLES (SMK, SEC) | P0 |
| M02 | Autenticación | Recuperación password | `/Auth/ForgotPassword`, `/Auth/ResetPassword` | AuthController | AllowAnonymous | — | PARCIAL | NO EXISTEN | P2 |
| M03 | Autenticación | Perfil | `/Auth/Profile` | AuthController | Authorize | — | FUNCIONAL | EXISTENTES INCOMPLETAS | P2 |
| M04 | Autenticación | AccessDenied | `/Auth/AccessDenied` | AuthController | AllowAnonymous | — | FUNCIONAL | EXISTENTES Y EJECUTABLES (SEC) | P1 |
| M05 | Usuarios | Listado/CRUD | `/User`, `/User/UserManagement` | UserController | UserManagement | — | FUNCIONAL | EXISTENTES INCOMPLETAS (SHF) | P1 |
| M06 | Usuarios | Alta admin | `/UserManagement` | UserManagementController | admin,superadmin | — | FUNCIONAL | NO EXISTEN | P2 |
| M07 | Usuarios | Asignaciones | `/UserAssignment` | UserAssignmentController | UserManagement | — | FUNCIONAL | EXISTENTES INCOMPLETAS (SHF) | P1 |
| M08 | Roles | Claim/role string | (sin UI roles CRUD) | Auth/policies Program.cs | — | — | SOLO BACKEND | EXISTENTES INCOMPLETAS (WTR/SEC) | P1 |
| M09 | Permisos | CheckPermission | `/Auth/CheckPermission` | AuthController | Authorize | — | SOLO BACKEND | NO EXISTEN | P2 |
| M10 | Empresas | CRUD | `/Company` | CompanyController | SystemConfig | — | FUNCIONAL | NO EXISTEN | P1 |
| M11 | Tenants | SuperAdmin | `/SuperAdmin` | SuperAdminController | superadmin | — | FUNCIONAL | EXISTENTES INCOMPLETAS (MT) | P0 |
| M12 | Sucursales | CRUD | `/Branch` | BranchController | admin | — | FUNCIONAL | NO EXISTEN | P1 |
| M13 | Configuración | Advanced Settings | `/AdvancedSettings` | AdvancedSettingsController | ManagerOrAbove | EnableAdvancedSettingsExtra (unused) | PARCIAL | NO EXISTEN | P2 |
| M14 | Configuración | Email | `/Email` | EmailController | SystemConfig | — | PARCIAL (sin Views/Email) | NO EXISTEN | P3 |
| M15 | Pisos/Áreas | CRUD | `/Area` | AreaController | admin,manager | — | FUNCIONAL | EXISTENTES Y EJECUTABLES (FLR) | P1 |
| M16 | Mesas | CRUD/estados | `/Table` | TableController | admin,manager,supervisor | — | FUNCIONAL | EXISTENTES Y EJECUTABLES (TBL) | P0 |
| M17 | Mesas | Merge/Split | TableController actions | TableController | admin,manager,supervisor | — | PARCIAL | EXISTENTES INCOMPLETAS | P2 |
| M18 | Estaciones | CRUD | `/Station` | StationController | admin,manager | — | FUNCIONAL | EXISTENTES Y EJECUTABLES (STN) | P0 |
| M19 | Meseros | RBAC waiter | Order + Waiters tests | policies OrderAccess | — | FUNCIONAL | EXISTENTES INCOMPLETAS (WTR) | P1 |
| M20 | Empleados | Users | `/User` | UserController | UserManagement | — | FUNCIONAL | EXISTENTES INCOMPLETAS | P2 |
| M21 | Turnos | Start/Handoff/End | `/Shift/*` POST | ShiftController | OrderAccess | — | SOLO BACKEND | EXISTENTES INCOMPLETAS (SHF) | P2 |
| M22 | POS | Pantalla principal | `/Order` | OrderController | OrderAccess | — | FUNCIONAL | EXISTENTES Y EJECUTABLES | P0 |
| M23 | Pedidos | CRUD/envío cocina | `/Order/*` | OrderController | OrderAccess | — | FUNCIONAL | EXISTENTES Y EJECUTABLES | P0 |
| M24 | KDS | Cocina | `/Order/StationOrders?stationType=kitchen` | OrderController | KitchenAccess | — | FUNCIONAL | EXISTENTES Y EJECUTABLES (KDS) | P0 |
| M25 | KDS | Bar | `/Order/StationOrders?stationType=bar` | OrderController | KitchenAccess | — | FUNCIONAL | EXISTENTES INCOMPLETAS | P1 |
| M26 | Productos | CRUD | `/Product` | ProductController | ProductAccess | — | FUNCIONAL | EXISTENTES INCOMPLETAS (REG) | P1 |
| M27 | Categorías | CRUD | `/Category` | CategoryController | SystemConfig | — | FUNCIONAL | NO EXISTEN | P2 |
| M28 | Modificadores | Modelo+seed | (sin controller UI) | ModifierService | — | — | SOLO BACKEND | NO EXISTEN | P2 |
| M29 | Combos | — | — | — | — | — | NO ACCESIBLE (no existe) | NO EXISTEN | — |
| M30 | Precios | Product.Price | Product | ProductController | ProductAccess | — | FUNCIONAL | EXISTENTES INCOMPLETAS | P1 |
| M31 | Impuestos | TaxRates | AdvancedSettings | AdvancedSettingsController | ManagerOrAbove | — | PARCIAL | NO EXISTEN | P2 |
| M32 | Descuentos | UI POS + policies | Order JS + DiscountPolicies | Order / AdvancedSettings | OrderAccess / Manager | — | PARCIAL | EXISTENTES INCOMPLETAS | P1 |
| M33 | Promociones | — | — | — | — | — | NO ACCESIBLE (no existe) | NO EXISTEN | — |
| M34 | Clientes | Entity+service | usado en Order | CustomerService | — | — | SOLO BACKEND / PARCIAL en POS | NO EXISTEN | P3 |
| M35 | Reservas | — | — | — | — | — | NO ACCESIBLE (no existe) | NO EXISTEN | — |
| M36 | Pagos | API | `api/Payment` | PaymentController | PaymentAccess | — | SOLO BACKEND + POS modal | EXISTENTES INCOMPLETAS (PAY) | P0 |
| M37 | Pagos | UI gestión | `/PaymentView` | PaymentViewController | PaymentAccess | — | FUNCIONAL | EXISTENTES Y EJECUTABLES | P1 |
| M38 | Pagos | Vista huérfana | `/Payment` view | (API PaymentController) | — | — | HUÉRFANO | NO EXISTEN | P4 |
| M39 | División cuenta | Person | `/Person/*` | PersonController | OrderAccess | — | PARCIAL (JS separate-accounts) | EXISTENTES INCOMPLETAS | P1 |
| M40 | Cambio mesa | Order ops | OrderController | OrderController | OrderAccess | — | PARCIAL | EXISTENTES INCOMPLETAS | P2 |
| M41 | Cancelaciones | Order cancel | OrderController | OrderController | OrderAccess | — | FUNCIONAL | EXISTENTES INCOMPLETAS (NEG) | P1 |
| M42 | Reembolsos | API refund | `api/Payment/refund` | PaymentController | PaymentAccess | — | SOLO BACKEND | NO EXISTEN | P1 |
| M43 | Caja RB-010 | Sesión | `/CashSession/*` | CashSessionController | CashAccess | EnableCashModule | FUNCIONAL | EXISTENTES Y EJECUTABLES | P0 |
| M44 | Caja | Registradoras | `/CashRegister` | CashRegisterController | CashAccess | EnableCashModule | FUNCIONAL | EXISTENTES Y EJECUTABLES | P1 |
| M45 | Caja | Movimientos API | `api/CashMovement` | CashMovementController | CashAccess | EnableCashModule | SOLO BACKEND | EXISTENTES INCOMPLETAS | P1 |
| M46 | Caja | Cierre X/Z | `/CashReport` | CashReportController | CashAccess | EnableCashModule | FUNCIONAL | EXISTENTES INCOMPLETAS | P0 |
| M47 | Caja | Reconciliación | CashSession/Reconciliation | CashSessionController | CashAccess | EnableCashModule | FUNCIONAL | EXISTENTES INCOMPLETAS | P0 |
| M48 | Inventario | Index/snapshot | `/Inventory` | InventoryController | InventoryAccess | — | FUNCIONAL | EXISTENTES Y EJECUTABLES | P0 |
| M49 | Inventario | Movimientos | InventoryMovementController | InventoryMovementController | InventoryAccess | — | SOLO BACKEND / JS | EXISTENTES INCOMPLETAS | P1 |
| M50 | Inventario | Stock assignment | `/ProductStockAssignment` | ProductStockAssignmentController | ProductAccess | — | FUNCIONAL | EXISTENTES INCOMPLETAS | P1 |
| M51 | Inventario | Transferencias | `/StockTransfer` | StockTransferController | InventoryAccess | — | PARCIAL | EXISTENTES INCOMPLETAS (INV-E) | P1 |
| M52 | Bodegas | master WMS | — | — | — | — | NO ACCESIBLE (no existe) | NO EXISTEN | — |
| M53 | Conteos físicos | — | — | — | — | — | NO ACCESIBLE (no existe) | NO EXISTEN | — |
| M54 | Mermas | Waste | FoodCost + InventoryMovement | FoodCostDashboard / InvMov | Costing/Inventory | EnableFoodCostModule | PARCIAL | EXISTENTES INCOMPLETAS | P2 |
| M55 | Recetas | CRUD/costo | `/Recipe` | RecipeController | CostingAccess | EnableFoodCostModule | FUNCIONAL | EXISTENTES INCOMPLETAS (FC) | P1 |
| M56 | Compras RB-020 | Dashboard | `/ProcurementDashboard` | ProcurementDashboardController | PurchasingAccess | EnablePurchasingModule | FUNCIONAL | EXISTENTES Y EJECUTABLES | P1 |
| M57 | Proveedores | CRUD | `/Supplier` | SupplierController | PurchasingAccess | EnablePurchasingModule | FUNCIONAL | EXISTENTES Y EJECUTABLES | P1 |
| M58 | Órdenes compra | Flujo | `/PurchaseOrder` | PurchaseOrderController | PurchasingAccess | EnablePurchasingModule | FUNCIONAL | EXISTENTES INCOMPLETAS | P0 |
| M59 | Solicitudes compra | Modelo | PurchaseRequest entities | (vía servicios) | PurchasingAccess | EnablePurchasingModule | PARCIAL / SOLO BACKEND | NO EXISTEN | P2 |
| M60 | Food Cost RB-023 | Dashboard | `/FoodCostDashboard` | FoodCostDashboardController | CostingAccess | EnableFoodCostModule | FUNCIONAL | EXISTENTES Y EJECUTABLES | P1 |
| M61 | Food Cost | Menu Engineering | `/FoodCostDashboard/MenuEngineering` | FoodCostDashboardController | CostingAccess | EnableFoodCostModule | FUNCIONAL | EXISTENTES Y EJECUTABLES | P2 |
| M62 | Analytics RB-025 | Centro Ejecutivo | `/ExecutiveAnalytics` | ExecutiveAnalyticsController | AnalyticsView | — | FUNCIONAL | EXISTENTES Y EJECUTABLES | P1 |
| M63 | Analytics | API | `api/analytics` | AnalyticsApiController | AnalyticsView | — | SOLO BACKEND | EXISTENTES INCOMPLETAS | P1 |
| M64 | Analytics | BI Nativo UI | `/BiNative` | BiNativeController | ReportAccess | — | FUNCIONAL | EXISTENTES INCOMPLETAS | P2 |
| M65 | Analytics | Command Center | `/ExecutiveCommandCenter` | ExecutiveCommandCenterController | ReportAccess | EnableCommandCenter | FUNCIONAL | EXISTENTES INCOMPLETAS (REG/PERF) | P2 |
| M66 | Analytics | Copilot | `/Copilot` | CopilotController | ReportAccess | EnableCopilot (OFF) | DESHABILITADO | NO EXISTEN | P3 |
| M67 | Reportes | Clásicos | `/Reports` | ReportsController | ReportAccess | — | PARCIAL (export stub) | NO EXISTEN | P2 |
| M68 | Reportes | Advanced | `/AdvancedReports` | AdvancedReportsController | ReportAccess | — | FUNCIONAL | NO EXISTEN | P2 |
| M69 | Exportaciones | Analytics CSV/XLSX/HTML | ExecutiveAnalytics/Export | ExecutiveAnalyticsController | AnalyticsExport | — | FUNCIONAL | EXISTENTES Y EJECUTABLES (AN-05) | P1 |
| M70 | Exportaciones | Reports PDF/Excel | ReportsController | ReportsController | ReportAccess | — | NO FUNCIONAL (stub) | NO EXISTEN | P2 |
| M71 | Alertas | BiAlert / VarianceAlert | (tablas + FC) | Intelligence / FoodCost | — | — | PARCIAL | NO EXISTEN | P3 |
| M72 | Notificaciones | Entity + settings | AdvancedSettings | models Notification | — | — | PARCIAL | NO EXISTEN | P3 |
| M73 | Auditoría | UI | `/Audit` | AuditController | ManagerOrAbove | — | FUNCIONAL | NO EXISTEN | P2 |
| M74 | Logs | AuditLog | Audit | AuditController | ManagerOrAbove | — | FUNCIONAL | NO EXISTEN | P2 |
| M75 | SaaS / Planes | Middleware suspensión | TenantSubscriptionMiddleware | Middleware | — | — | PARCIAL (activo/inactivo) | NO EXISTEN | P2 |
| M76 | Suscripciones | billing UI | — | — | — | — | NO ACCESIBLE (no billing UI) | NO EXISTEN | — |
| M77 | Facturación | Invoice entity | (parcial) | InvoiceService | — | — | SOLO BACKEND | NO EXISTEN | P3 |
| M78 | Integraciones/Webhooks | — | — | — | — | — | NO ACCESIBLE | NO EXISTEN | — |
| M79 | API kitchen | `api/kitchen` | KitchenApiController | KitchenAccess | — | SOLO BACKEND | EXISTENTES INCOMPLETAS | P2 |
| M80 | Documentos | — | — | — | — | — | NO ACCESIBLE | NO EXISTEN | — |
| M81 | Localización | — | — | — | — | — | NO ACCESIBLE (ES hardcode) | NO EXISTEN | — |
| M82 | Responsive | viewport projects | Playwright projects | — | — | FUNCIONAL (parcial) | EXISTENTES Y EJECUTABLES (RSP) | P2 |
| M83 | Dark Mode | — | — | — | — | — | NO ACCESIBLE (no theme) | NO EXISTEN | — |
| M84 | Seed / Demo | `/Seed` | SeedController | Dev only | EnableSeedEndpoints (unused; env gate) | FUNCIONAL (Dev) | NO EXISTEN | P3 |
| M85 | Home Dashboard | `/Home` | HomeController | Authorize | — | FUNCIONAL | EXISTENTES Y EJECUTABLES | P1 |
| M86 | SignalR | `/orderHub` | OrderHub | Authorize | — | FUNCIONAL | EXISTENTES INCOMPLETAS | P1 |

## Módulos solicitados y NO EXISTENTES (código ejecutable)

Combos · Promociones · Reservas · Bodegas WMS · Conteos físicos · Webhooks · Billing/Planes SaaS UI · Dark Mode · Localización i18n · Documentos.

## Resumen conteo

| Estado UI | Cantidad aprox. |
|-----------|-----------------|
| FUNCIONAL | 45+ |
| PARCIAL | 18+ |
| SOLO BACKEND | 12+ |
| NO ACCESIBLE / no existe | 12+ |
| HUÉRFANO / DESHABILITADO / stub | 4 |

| Pruebas browser | Cantidad aprox. |
|-----------------|-----------------|
| EXISTENTES Y EJECUTABLES | ~25 módulos cubiertos parcialmente |
| EXISTENTES INCOMPLETAS | ~30 |
| NO EXISTEN | ~30 (incl. no-aplicables) |
