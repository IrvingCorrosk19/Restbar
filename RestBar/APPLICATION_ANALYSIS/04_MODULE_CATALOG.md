# 04 — Module Catalog

**Sistema:** RestBar  
**Fecha:** 2026-07-04

Catálogo completo de módulos funcionales descubiertos en el código (no asumidos).

---

## M01 — Autenticación y Sesión

| Aspecto | Detalle |
|---------|---------|
| **Objetivo** | Identificar usuarios, establecer sesión, emitir claims |
| **Responsabilidad** | Login/logout, perfil, recuperación de contraseña, verificación de permisos |
| **Controlador** | `AuthController` |
| **Servicios** | `AuthService`, `UserService`, `EmailService` |
| **Vistas** | `Auth/Login`, `Auth/Profile`, `Auth/AccessDenied` |
| **Modelos DB** | `users` |
| **Políticas** | AllowAnonymous (login), Authorize (profile) |
| **Dependencias** | EmailService (recuperación), RestBarContext |

**Endpoints principales:** `GET/POST /Auth/Login`, `POST /Auth/Logout`, `GET /Auth/Profile`, `GET /Auth/CurrentUser`, `GET /Auth/AccessDenied`, recuperación contraseña.

---

## M02 — Dashboard

| Aspecto | Detalle |
|---------|---------|
| **Objetivo** | Punto de entrada post-login con navegación por rol |
| **Responsabilidad** | Mostrar cards de módulos según rol del usuario |
| **Controlador** | `HomeController` |
| **Vistas** | `Home/Index`, `Home/Privacy`, `Shared/Error` |
| **Lógica** | `GetVisibleCardsForRole()` — 11 roles con matrices de visibilidad |
| **Dependencias** | Claims (UserRole, BranchName, CompanyName), StationService (tipos dinámicos) |

---

## M03 — POS / Órdenes

| Aspecto | Detalle |
|---------|---------|
| **Objetivo** | Toma de pedidos, gestión del ciclo de vida de órdenes |
| **Responsabilidad** | Crear/editar órdenes, agregar/quitar ítems, enviar a cocina, completar, cancelar |
| **Controlador** | `OrderController` |
| **Servicios** | `OrderService`, `OrderItemService`, `CategoryService`, `ProductService`, `TableService`, `AreaService`, `CustomerService`, `OrderHubService`, `EmailService` |
| **Vistas** | `Order/Index` + 5 partials |
| **Layout** | `_OrderLayout` |
| **JS** | 12 módulos en `wwwroot/js/order/` |
| **Modelos DB** | `orders`, `order_items`, `tables`, `products`, `categories` |
| **Política** | `OrderAccess` |
| **Estados** | Pending → SentToKitchen → Preparing → Ready → ReadyToPay → Served → Completed / Cancelled |

**Endpoints clave:** Create, AddItems, RemoveItem, UpdateItemQuantity, SendToKitchen, MarkItemReady, CompleteOrder, Cancel, GetActiveOrder, GetActiveTables, GetProductsByCategory.

---

## M04 — Cocina / KDS (Kitchen Display System)

| Aspecto | Detalle |
|---------|---------|
| **Objetivo** | Visualizar y gestionar órdenes por estación de preparación |
| **Responsabilidad** | Mostrar ítems pendientes, marcar como listos, notificar en tiempo real |
| **Controladores** | `OrderController` (StationOrders), `KitchenApiController` |
| **Servicios** | `KitchenService`, `OrderService`, `StationService`, `OrderHubService` |
| **Vistas** | `Order/StationOrders` |
| **Layout** | `_KitchenLayout` |
| **API** | `GET /api/kitchen/current` (snapshot post-reconexión) |
| **Política** | `KitchenAccess` (API), `OrderAccess` + permiso "kitchen" (MVC) |
| **Dependencias** | ProductStockAssignment (productos sin asignación no aparecen) |

---

## M05 — Pagos

| Aspecto | Detalle |
|---------|---------|
| **Objetivo** | Registrar pagos parciales/completos, split, anulación |
| **Responsabilidad** | Procesar pagos, actualizar estado orden/mesa, notificar SignalR |
| **Controladores** | `PaymentController` (API), `PaymentViewController` (UI) |
| **Servicios** | `PaymentService`, `SplitPaymentService`, `OrderService`, `OrderHubService`, `EmailService` |
| **Vistas** | `PaymentView/Index` |
| **JS** | `payments.js`, `payment-management.js` |
| **Modelos DB** | `payments`, `split_payments` |
| **Política** | `PaymentAccess` |
| **Métodos de pago** | Efectivo, Tarjeta, Transferencia, Compartido |

**API:** `POST /api/Payment/partial`, `GET /api/Payment/order/{id}/summary`, `DELETE /api/Payment/{id}` (void).

---

## M06 — Cuentas Separadas (Split Bill)

| Aspecto | Detalle |
|---------|---------|
| **Objetivo** | Dividir cuenta entre comensales |
| **Responsabilidad** | Crear personas por orden, asignar ítems, pagos individuales |
| **Controlador** | `PersonController` |
| **Servicios** | `PersonService`, `OrderService` |
| **JS** | `separate-accounts-simple.js` (global), `separate-accounts.js` (completo, no cargado) |
| **CSS** | `separate-accounts.css` |
| **Modelos DB** | `persons` |
| **Política** | `OrderAccess` |

---

## M07 — Mesas

| Aspecto | Detalle |
|---------|---------|
| **Objetivo** | Gestionar mesas físicas del restaurante |
| **Responsabilidad** | CRUD mesas, estados, liberar mesas fantasma |
| **Controlador** | `TableController` |
| **Servicios** | `TableService`, `AreaService` |
| **Vista** | `Table/Index` |
| **Modelos DB** | `tables` |
| **Roles** | admin, manager, supervisor |
| **Estados** | Disponible, Ocupada, Reservada, EnEspera, Atendida, EnPreparacion, Servida, ParaPago, Pagada, Bloqueada |

---

## M08 — Áreas

| Aspecto | Detalle |
|---------|---------|
| **Objetivo** | Organizar el espacio físico en zonas |
| **Responsabilidad** | CRUD áreas (Salón, Terraza, etc.) |
| **Controlador** | `AreaController` |
| **Servicios** | `AreaService`, `BranchService` |
| **Vista** | `Area/Index` |
| **Modelos DB** | `areas` |
| **Roles** | admin, manager |

---

## M09 — Productos

| Aspecto | Detalle |
|---------|---------|
| **Objetivo** | Catálogo de productos del menú |
| **Responsabilidad** | CRUD productos, verificación de stock, precios |
| **Controlador** | `ProductController` |
| **Servicios** | `ProductService`, `CategoryService`, `StationService` |
| **Vista** | `Product/Index` |
| **ViewModels** | `ProductCreateViewModel`, `ProductEditViewModel` |
| **Modelos DB** | `products`, `categories` |
| **Política** | `ProductAccess` |

---

## M10 — Categorías

| Aspecto | Detalle |
|---------|---------|
| **Objetivo** | Organizar productos en categorías de menú |
| **Responsabilidad** | CRUD categorías |
| **Controlador** | `CategoryController` |
| **Servicios** | `CategoryService`, `ProductService` |
| **Vistas** | `Category/Index`, `Category/Edit` |
| **Modelos DB** | `categories` (activo), `product_categories` (legacy) |
| **Política** | `SystemConfig` |

---

## M11 — Estaciones

| Aspecto | Detalle |
|---------|---------|
| **Objetivo** | Definir puntos de preparación (cocina, bar, etc.) |
| **Responsabilidad** | CRUD estaciones, tipos, asociación con áreas |
| **Controlador** | `StationController` |
| **Servicios** | `StationService`, `AreaService` |
| **Vistas** | `Station/Index`, Create, Edit, Delete, Details |
| **Modelos DB** | `stations` |
| **Roles** | admin, manager |

---

## M12 — Inventario

| Aspecto | Detalle |
|---------|---------|
| **Objetivo** | Control de stock y alertas de inventario bajo |
| **Responsabilidad** | Monitorear stock, reportes de consumo |
| **Controlador** | `InventoryController` |
| **Servicios** | `ProductService`, `AreaService` + RestBarContext directo |
| **Vista** | `Inventory/Index` |
| **JS** | `inventory-management.js` |
| **CSS** | `inventory.css` |
| **Modelos DB** | `products` (stock), `product_stock_assignments` |
| **Política** | `InventoryAccess` |

---

## M13 — Asignación Stock-Estación

| Aspecto | Detalle |
|---------|---------|
| **Objetivo** | Asignar stock de productos a estaciones específicas |
| **Responsabilidad** | CRUD asignaciones producto↔estación |
| **Controlador** | `ProductStockAssignmentController` |
| **Servicios** | `ProductStockAssignmentService` |
| **Vista** | `ProductStockAssignment/Index` |
| **JS** | `product-stock-assignment.js` |
| **Modelos DB** | `product_stock_assignments` |
| **Política** | `ProductAccess` |

---

## M14 — Usuarios

| Aspecto | Detalle |
|---------|---------|
| **Objetivo** | Gestión de personal del restaurante |
| **Responsabilidad** | CRUD usuarios, asignación a sucursal |
| **Controladores** | `UserController`, `UserManagementController` |
| **Servicios** | `UserService`, `GlobalLoggingService` |
| **Vistas** | `User/Index`, `User/UserManagement`, `UserManagement/Index`, `UserManagement/Create` |
| **Modelos DB** | `users` |
| **Política** | `UserManagement` |

---

## M15 — Asignaciones de Personal

| Aspecto | Detalle |
|---------|---------|
| **Objetivo** | Asignar personal a áreas, estaciones y mesas |
| **Responsabilidad** | CRUD asignaciones usuario↔recurso |
| **Controlador** | `UserAssignmentController` |
| **Servicios** | `UserAssignmentService` |
| **Vista** | `UserAssignment/Index` |
| **Modelos DB** | `user_assignments` |
| **Política** | `UserManagement` |

---

## M16 — Compañías (Multi-Tenant)

| Aspecto | Detalle |
|---------|---------|
| **Objetivo** | Gestión de empresas tenant |
| **Responsabilidad** | CRUD compañías |
| **Controlador** | `CompanyController` |
| **Servicios** | `CompanyService` |
| **Vista** | `Company/Index` |
| **Modelos DB** | `companies` |
| **Política** | `SystemConfig` |

---

## M17 — Sucursales

| Aspecto | Detalle |
|---------|---------|
| **Objetivo** | Gestión de sucursales por compañía |
| **Responsabilidad** | CRUD sucursales |
| **Controlador** | `BranchController` |
| **Servicios** | `BranchService` |
| **Vista** | `Branch/Index` |
| **Modelos DB** | `branches` |
| **Roles** | admin |

---

## M18 — SuperAdmin

| Aspecto | Detalle |
|---------|---------|
| **Objetivo** | Administración global del SaaS |
| **Responsabilidad** | Gestionar compañías, sucursales y admins cross-tenant |
| **Controlador** | `SuperAdminController` |
| **Vistas** | Index, Companies, Branches, Create/Edit Company/Branch/Admin |
| **Modelos DB** | companies, branches, users |
| **Rol** | superadmin exclusivo |

---

## M19 — Reportes Básicos

| Aspecto | Detalle |
|---------|---------|
| **Objetivo** | Reportes de ventas estándar |
| **Responsabilidad** | Métricas, ventas diarias, exportación |
| **Controlador** | `ReportsController` |
| **Servicios** | `SalesReportService` |
| **Vista** | `Reports/Index` |
| **Política** | `ReportAccess` |

---

## M20 — Reportes Avanzados

| Aspecto | Detalle |
|---------|---------|
| **Objetivo** | Analytics avanzados del negocio |
| **Responsabilidad** | Rentabilidad, top productos, clientes, tendencias, auditoría, salud del sistema |
| **Controlador** | `AdvancedReportsController` |
| **Servicios** | `AdvancedReportsService` |
| **Vistas** | 10 páginas de reporte + Index |
| **JS** | `advanced-reports.js`, `inventory-analysis.js` (+ 3 JS faltantes) |
| **Política** | `ReportAccess` |

**Sub-reportes:** Profitability, Sales, Customer, Operational, Inventory, Supplier, Trend, Audit, SystemHealth.

---

## M21 — Configuración Avanzada

| Aspecto | Detalle |
|---------|---------|
| **Objetivo** | Parámetros operativos del sistema |
| **Responsabilidad** | Moneda, impuestos, descuentos, horarios, notificaciones, backup |
| **Controlador** | `AdvancedSettingsController` |
| **Servicios** | SystemSettings, Currency, TaxRate, DiscountPolicy, OperatingHours, NotificationSettings, BackupSettings |
| **Vistas** | Index, SystemSettings (+ vistas referenciadas sin implementar) |
| **Política** | `ManagerOrAbove` |

---

## M22 — Email y Plantillas

| Aspecto | Detalle |
|---------|---------|
| **Objetivo** | Comunicación por correo electrónico |
| **Responsabilidad** | Plantillas, prueba SMTP, envío transaccional |
| **Controlador** | `EmailController` |
| **Servicios** | `EmailService`, `EmailTemplateService` |
| **Modelos DB** | `email_templates` |
| **Política** | `SystemConfig` |
| **Estado** | Deshabilitado por defecto (`Email:Enabled = false`) |

---

## M23 — Auditoría

| Aspecto | Detalle |
|---------|---------|
| **Objetivo** | Trazabilidad de acciones del sistema |
| **Responsabilidad** | Visualizar logs de auditoría filtrados |
| **Controlador** | `AuditController` |
| **Servicios** | `AuditLogService` |
| **Vistas** | `Audit/Index`, `Audit/Details` |
| **Modelos DB** | `audit_logs`, `order_cancellation_logs` |
| **Política** | `[Authorize]` (cualquier autenticado) |

---

## M24 — Notificaciones (Tiempo Real)

| Aspecto | Detalle |
|---------|---------|
| **Objetivo** | Actualizaciones en tiempo real entre pantallas |
| **Responsabilidad** | Push de eventos vía SignalR |
| **Hub** | `OrderHub` (`/orderHub`) |
| **Servicio** | `OrderHubService` |
| **Entidad DB** | `notifications` (uso limitado en controllers) |
| **Eventos** | 9 tipos de eventos (ver Architecture doc) |

---

## M25 — Clientes

| Aspecto | Detalle |
|---------|---------|
| **Objetivo** | Gestión básica de clientes |
| **Responsabilidad** | Entidad con loyalty points |
| **Servicio** | `CustomerService` |
| **Modelos DB** | `customers` |
| **Estado UI** | Sin controlador/vista dedicada; usado desde OrderController |

---

## M26 — Facturación

| Aspecto | Detalle |
|---------|---------|
| **Objetivo** | Generación de facturas |
| **Servicio** | `InvoiceService` |
| **Modelos DB** | `invoices` |
| **Estado** | Entidad y servicio existen; sin flujo UI completo |

---

## M27 — Modificadores

| Aspecto | Detalle |
|---------|---------|
| **Objetivo** | Extras/modificadores de productos |
| **Servicio** | `ModifierService` |
| **Modelos DB** | `modifiers`, `product_modifiers` (M:N) |
| **Estado** | Entidad existe; sin UI de gestión |

---

## M28 — Seed / Utilidades Dev

| Aspecto | Detalle |
|---------|---------|
| **Objetivo** | Poblar datos de demostración |
| **Controlador** | `SeedController` |
| **Vista** | `Seed/Index` |
| **Estado** | Bloqueado en producción para SeedDemoData; endpoints CreateAdmin con riesgo |

---

## Módulos NO Implementados (descubiertos como gaps)

| Módulo | Evidencia |
|--------|-----------|
| **Proveedores** | `supplier-management.js` sin controller |
| **Compras** | Sin entidad ni controller |
| **Impresoras** | Vista referenciada en AdvancedSettings sin implementar |
| **Contabilidad UI** | `accounting.js` sin vista |
| **Promociones** | Solo DiscountPolicy en config + modal descuento en POS |
| **Backup automático** | BackupSettings almacena cron pero sin worker |

---

## Matriz Resumen

| Módulo | Controller | Service(s) | Vista | DB | Política/Rol |
|--------|-----------|------------|-------|-----|-------------|
| Auth | AuthController | AuthService | ✅ | users | Mixed |
| Dashboard | HomeController | — | ✅ | — | Authorize |
| POS | OrderController | OrderService +6 | ✅ | orders, order_items | OrderAccess |
| KDS | OrderController, KitchenApiController | KitchenService | ✅ | — | KitchenAccess |
| Pagos | PaymentController, PaymentViewController | PaymentService | ✅ | payments | PaymentAccess |
| Split Bill | PersonController | PersonService | — (JS) | persons | OrderAccess |
| Mesas | TableController | TableService | ✅ | tables | admin,manager,supervisor |
| Áreas | AreaController | AreaService | ✅ | areas | admin, manager |
| Productos | ProductController | ProductService | ✅ | products | ProductAccess |
| Categorías | CategoryController | CategoryService | ✅ | categories | SystemConfig |
| Estaciones | StationController | StationService | ✅ | stations | admin, manager |
| Inventario | InventoryController | ProductService | ✅ | products | InventoryAccess |
| Stock Assign | ProductStockAssignmentController | ProductStockAssignmentService | ✅ | product_stock_assignments | ProductAccess |
| Usuarios | UserController, UserManagementController | UserService | ✅ | users | UserManagement |
| Asignaciones | UserAssignmentController | UserAssignmentService | ✅ | user_assignments | UserManagement |
| Compañías | CompanyController | CompanyService | ✅ | companies | SystemConfig |
| Sucursales | BranchController | BranchService | ✅ | branches | admin |
| SuperAdmin | SuperAdminController | — (direct DB) | ✅ | — | superadmin |
| Reportes | ReportsController | SalesReportService | ✅ | — | ReportAccess |
| Rep. Avanzados | AdvancedReportsController | AdvancedReportsService | ✅ | — | ReportAccess |
| Config | AdvancedSettingsController | 7 services | Parcial | settings tables | ManagerOrAbove |
| Email | EmailController | EmailService | ❌ | email_templates | SystemConfig |
| Auditoría | AuditController | AuditLogService | ✅ | audit_logs | Authorize |
| SignalR | OrderHub | OrderHubService | — | — | Sin auth hub |
| Seed | SeedController | — | ✅ | — | AllowAnonymous |

---

*Catálogo de módulos descubierto en análisis de código. Sin modificaciones al sistema.*
