# Auditoría funcional previa a pruebas E2E — RestBar

**Documento:** Mapeo completo de módulos funcionales  
**Propósito:** Identificar todos los módulos, endpoints, roles, dependencias y riesgos antes de ejecutar pruebas End-to-End.  
**Fecha:** 2025-02-27  
**Alcance:** Sistema POS / SaaS multiusuario (finanzas, órdenes, cocina, inventario, reportes).

---

## 1. Resumen de la estructura del proyecto

### 1.1 Stack y configuración

| Elemento | Detalle |
|----------|---------|
| **Framework** | ASP.NET Core MVC + API Controllers |
| **Base de datos** | PostgreSQL (Npgsql), EF Core |
| **Autenticación** | Cookie (CookieAuthenticationDefaults), claims (UserId, UserRole, BranchId, etc.) |
| **Autorización** | Políticas por rol (AdminOnly, OrderAccess, PaymentAccess, KitchenAccess, etc.) |
| **Tiempo real** | SignalR (`OrderHub` en `/orderHub`) |
| **Sesión** | DistributedMemoryCache + Session (30 min idle) |
| **Cultura** | es-PA (Panamá), fechas dd/MM/yyyy |
| **Migraciones** | 4 migraciones: InitialCreate, AddAllowNegativeStockToProducts, RemoveStationIdFromProducts, AddOrderVersionConcurrencyToken |

### 1.2 Controllers (25)

- **AuthController** — Login, Logout, Profile, CurrentUser, AccessDenied, recuperación contraseña  
- **HomeController** — Dashboard por rol  
- **OrderController** — Órdenes, ítems, envío a cocina, KDS (StationOrders), pagos (historial)  
- **PaymentController** (API) — Pagos parciales, resumen, anulación  
- **PaymentViewController** — Vistas de pagos, estadísticas, recientes  
- **TableController** — Mesas CRUD, áreas, liberar mesas fantasma  
- **AreaController** — Áreas CRUD  
- **StationController** — Estaciones CRUD, tipos, API  
- **ProductController** — Productos y categorías CRUD, stock por estación  
- **ProductStockAssignmentController** — Asignación producto–estación  
- **InventoryController** — Stock bajo, datos inventario, reporte consumo  
- **CategoryController** — Categorías CRUD  
- **CompanyController** — Compañías (multi-tenant)  
- **BranchController** — Sucursales  
- **UserController** — Usuarios CRUD, API  
- **UserManagementController** — Vista gestión usuarios (admin/superadmin)  
- **UserAssignmentController** — Asignaciones usuario–estación/mesa  
- **PersonController** — Personas por orden (cuentas separadas)  
- **ReportsController** — Reportes de ventas, métricas, exportación  
- **AdvancedReportsController** — Rentabilidad, top productos/categorías, clientes, estaciones, mesas, inventario, auditoría, export PDF/Excel  
- **AdvancedSettingsController** — SystemSettings, Currency, TaxRate, DiscountPolicy, OperatingHours, NotificationSettings, BackupSettings  
- **AuditController** — Logs de auditoría filtrados  
- **EmailController** — Plantillas, prueba SMTP  
- **SuperAdminController** — Dashboard multi-tenant, compañías, sucursales, admins  
- **SeedController** — Semilla y utilidades (AllowAnonymous, solo desarrollo)  

### 1.3 Middlewares

| Middleware | Orden | Función |
|------------|--------|---------|
| **UseAuditLogging** | Tras auth | Registra REQUEST_START/REQUEST_SUCCESS y errores en AuditLog |
| **UseErrorHandling** | Tras auth | Captura excepciones, devuelve JSON 4xx/5xx, registra en AuditLog |
| **UsePermissionValidation** | Tras auth | Valida ruta vs permisos (AuthService.HasPermissionAsync); SuperAdmin bypass |

Rutas excluidas de PermissionMiddleware: `/Auth`, `/Home/Error`, `/`, estáticos (css, js, images, lib, favicon).

### 1.4 Políticas de autorización (Program.cs)

| Política | Roles |
|----------|--------|
| AdminOnly | admin |
| ManagerOrAbove | admin, manager |
| SupervisorOrAbove | admin, manager, supervisor |
| OrderAccess | admin, manager, supervisor, waiter, cashier |
| KitchenAccess | admin, manager, supervisor, chef, bartender |
| PaymentAccess | admin, manager, supervisor, cashier, accountant |
| InventoryAccess | admin, manager, supervisor, accountant, **inventarista** |
| ProductAccess | admin, manager |
| UserManagement | admin, manager, support |
| ReportAccess | admin, manager, accountant |
| AccountingAccess | admin, manager, accountant |
| SystemConfig | admin |

**Nota:** El rol `inventarista` está en política y en HomeController pero **no** está definido en el enum `UserRole` (Models/User.cs). Riesgo de inconsistencia.

### 1.5 Entidades principales (RestBarContext)

- **Multi-tenant:** Company, Branch, Area, User, UserAssignment  
- **Operación:** Order (Version para concurrencia), OrderItem, Table, Product, ProductCategory, ProductStockAssignment, Station, Category  
- **Finanzas:** Payment, SplitPayment, Invoice  
- **Personas:** Person (por orden), Customer  
- **Sistema:** AuditLog, Notification, Modifier  
- **Configuración:** SystemSettings, Currency, TaxRate, DiscountPolicy, OperatingHours, NotificationSettings, BackupSettings, EmailTemplate  
- **Auditoría:** OrderCancellationLog  

### 1.6 Background services

- **No hay** `IHostedService` ni `BackgroundService` registrados. No existe outbox, jobs programados ni workers en este proyecto.

---

## 2. Módulos funcionales por dominio

### 2.1 Autenticación

| Aspecto | Detalle |
|---------|---------|
| **Propósito** | Login/logout por cookie, claims (UserId, UserRole, BranchId, etc.), redirección por rol, perfil y API CurrentUser. |
| **Endpoints** | `GET/POST /Auth/Login`, `POST /Auth/Logout`, `GET /Auth/AccessDenied`, `GET /Auth/Profile`, `GET /Auth/CurrentUser`, y endpoints de recuperación de contraseña (GET/POST). |
| **Roles** | AllowAnonymous en Login/AccessDenied/recuperación; Authorize en Profile y CurrentUser. |
| **Dependencias** | AuthService, UserService, EmailService (recuperación), RestBarContext. |
| **Riesgos** | Credenciales en log (Console); sesión 8h con sliding; cookie "RestBarAuth" HttpOnly/SameSite Lax. |

---

### 2.2 Usuarios

| Aspecto | Detalle |
|---------|---------|
| **Propósito** | CRUD de usuarios, filtro por rol/sucursal/activo, asignación a sucursal. |
| **Endpoints** | `GET/POST /User/GetUsers`, `GET /User/GetUser/{id}`, `POST /User/Create`, `POST /User/Update`, `DELETE /User/Delete/{id}`, y otros GET para branches/roles (autorizados). |
| **Roles** | Policy **UserManagement** (admin, manager, support). Algunos endpoints solo [Authorize]. |
| **Dependencias** | UserService, GlobalLoggingService, RestBarContext, IHttpContextAccessor. |
| **Riesgos** | Password en form; posible null en Branch/Company en vistas. |

---

### 2.3 Roles

| Aspecto | Detalle |
|---------|---------|
| **Propósito** | Definidos en enum `UserRole`: superadmin, admin, manager, supervisor, waiter, cashier, chef, bartender, accountant, support. No hay CRUD de roles; son fijos. |
| **Endpoints** | No hay controlador de roles; se usan en políticas y en User.Role. |
| **Riesgos** | Política InventoryAccess usa "inventarista" que no existe en UserRole → usuarios con ese rol no se pueden crear desde el enum. |

---

### 2.4 Órdenes

| Aspecto | Detalle |
|---------|---------|
| **Propósito** | Crear/editar órdenes, agregar/quitar/actualizar ítems, enviar a cocina, marcar ítems listos, completar orden, cancelar, ver historial de pagos. Estados: Pending, SentToKitchen, Preparing, ReadyToPay, Served, Completed, Cancelled. |
| **Endpoints** | `GET/POST /Order/Index`, `POST /Order/Create`, `POST /Order/Edit/{id}`, `POST /Order/AddItem`, `POST /Order/AddItems`, `POST /Order/RemoveItemFromOrder`, `POST /Order/UpdateItemQuantityInOrder`, `POST /Order/UpdateItemInOrder`, `POST /Order/UpdateItemStatus`, `POST /Order/SendToKitchen`, `POST /Order/MarkItemReady`, `POST /Order/CompleteOrder`, `POST /Order/Cancel`, `POST /Order/SetTableOccupied`, `POST /Order/CheckAndUpdateTableStatus`, `POST /Order/UpdateOrderComplete`, `GET /Order/GetActiveOrder/{tableId}`, `GET /Order/GetActiveCategories`, `GET /Order/GetProductsByCategory/{id}`, `GET /Order/GetActiveTables`, `GET /Order/GetAreas`, `GET /Order/GetCategories`, `GET /Order/GetProducts`, `GET /Order/GetPaymentHistory/{orderId}`, `GET /Order/CheckItemStockAvailability`, `GET /Order/GetOrderItems/{orderId}`, `GET /Order/Test`. |
| **Roles** | Policy **OrderAccess** (admin, manager, supervisor, waiter, cashier). |
| **Dependencias** | OrderService, OrderItemService, CategoryService, ProductService, TableService, AreaService, CustomerService, UserService, OrderHubService, EmailService, RestBarContext. |
| **Riesgos** | Concurrencia en Order (Version); DbUpdateConcurrencyException si dos usuarios actualizan la misma orden; null en OrderItems/Table/Customer. |

---

### 2.5 Pagos

| Aspecto | Detalle |
|---------|---------|
| **Propósito** | Pagos parciales y completos, split por persona, anulación (void), resumen por orden. Actualización de estado de orden y mesa al pagar. |
| **Endpoints (API)** | `POST /api/Payment/partial`, `GET /api/Payment/order/{orderId}/summary`, `GET /api/Payment/order/{orderId}`, `DELETE /api/Payment/{paymentId}` (void). |
| **Endpoints (vistas)** | `GET /PaymentView/Index`, `GET /PaymentView/DashboardStats`, `GET /PaymentView/RecentPayments`, y otras vistas de listado/filtros. |
| **Roles** | Policy **PaymentAccess** (admin, manager, supervisor, cashier, accountant). |
| **Dependencias** | PaymentService, SplitPaymentService, OrderService, RestBarContext, OrderHubService, ProductService, EmailService. |
| **Riesgos** | **Crítico financiero:** doble pago si no se valida totalPaid dentro de transacción (ya mitigado con re-lectura en transacción). Overpay manejado en summary (WarningCode OVERPAID). VoidPayment puede dejar orden en estado inconsistente si falla después de anular. DbUpdateConcurrencyException devuelta como 409. |

---

### 2.6 Cocina (KDS)

| Aspecto | Detalle |
|---------|---------|
| **Propósito** | Pantalla de órdenes por estación (kitchen, bar, etc.), marcar ítems listos, notificaciones en tiempo real vía SignalR. |
| **Endpoints** | `GET /Order/StationOrders?stationType=kitchen` (y otros tipos), `GET /Order/KitchenOrders` (redirige a StationOrders), `GET /Order/BarOrders` (redirige), `POST /Order/MarkItemReady`. |
| **Roles** | OrderAccess (mismo que órdenes); PermissionMiddleware mapea `/stationorders` a permiso "kitchen". |
| **Dependencias** | OrderService (GetKitchenOrdersAsync, MarkItemReady), StationService, OrderHubService. |
| **Riesgos** | Productos sin ProductStockAssignment no aparecen en KDS; ítems sin estación configurada. Posible null en PreparedByStation o Items. |

---

### 2.7 Inventario

| Aspecto | Detalle |
|---------|---------|
| **Propósito** | Productos con stock bajo, datos de inventario por producto/sucursal, reporte de consumo por fechas/estación. |
| **Endpoints** | `GET /Inventory/Index`, `GET /Inventory/GetLowStockProducts`, `GET /Inventory/GetInventoryData`, `GET /Inventory/GetProducts`, `GET /Inventory/GetCategories`, `GET /Inventory/GetBranches`, `GET /Inventory/ConsumptionReport`. |
| **Roles** | Policy **InventoryAccess** (admin, manager, supervisor, accountant, inventarista). |
| **Dependencias** | RestBarContext, ProductService, AreaService. |
| **Riesgos** | Filtro por BranchId/CompanyId; si usuario sin sucursal, posibles listas vacías o errores. |

---

### 2.8 Mesas y áreas

| Aspecto | Detalle |
|---------|---------|
| **Propósito** | CRUD mesas (por área/sucursal), CRUD áreas, liberar mesas “fantasma” (Ocupada sin orden activa). |
| **Endpoints (Table)** | `GET /Table/GetTables`, `GET /Table/GetAreas`, `GET /Table/Get/{id}`, `GET /Table/FixTableStatus`, `POST /Table/Create`, `PUT /Table/Edit/{id}`, `DELETE /Table/Delete/{id}`, `POST /Table/ReleaseGhostTables`. |
| **Endpoints (Area)** | `GET /Area/GetAreas`, `GET /Area/Get/{id}`, `GET /Area/GetBranches`, `POST /Area/Create`, `POST /Area/CreateAjax`, `PUT /Area/Edit/{id}`, `DELETE /Area/Delete/{id}`. |
| **Roles** | Table: **admin, manager, supervisor**. Area: **admin, manager**. |
| **Dependencias** | TableService, AreaService, BranchService, RestBarContext. |
| **Riesgos** | ReleaseGhostTables puede afectar muchas mesas; FixTableStatus es operación correctiva sensible. |

---

### 2.9 Productos, categorías y estaciones

| Aspecto | Detalle |
|---------|---------|
| **Propósito** | Productos y categorías CRUD, asignación producto–estación (stock por estación), estaciones CRUD y tipos. |
| **Endpoints (Product)** | `GET /Product/GetProducts`, `GET /Product/GetCategories`, `POST /Product/Create`, `PUT /Product/Edit/{id}`, `DELETE /Product/Delete/{id}`, `POST /Product/CreateCategoryAjax`, `GET /Product/Get/{id}`, `GET /Product/GetAvailableStock`, `GET /Product/GetStockInStation`, `GET /Product/CheckStockAvailability`, `GET /Product/FindBestStation`. |
| **Endpoints (ProductStockAssignment)** | `GET /ProductStockAssignment/GetAssignments`, `POST /ProductStockAssignment/Create`, `PUT /ProductStockAssignment/Update/{id}`, `DELETE /ProductStockAssignment/Delete/{id}`. |
| **Endpoints (Station)** | `GET /Station/DetailsAjax/{id}`, `POST /Station/Create`, `POST /Station/CreateAjax`, `POST /Station/Edit/{id}`, `POST /Station/EditAjax/{id}`, `POST /Station/DeleteAjax/{id}`, `GET /Station/GetStations`, `GET /Station/GetStationTypes`, `GET /Station/GetStationById/{id}`, `GET /Station/GetAreas`. |
| **Endpoints (Category)** | `GET /Category/Index`, `GET/POST /Category/Create`, `GET/POST /Category/Edit/{id}`, `GET/POST /Category/Delete/{id}`. |
| **Roles** | Product y ProductStockAssignment: **ProductAccess** (admin, manager). Station: **admin, manager**. Category: **SystemConfig** (admin). |
| **Dependencias** | ProductService, CategoryService, AreaService, StationService, ProductStockAssignmentService, RestBarContext. |
| **Riesgos** | Productos con AllowNegativeStock y sin asignación de estación afectan KDS e inventario. Eliminar categoría con productos puede fallar por FK. |

---

### 2.10 Reportes

| Aspecto | Detalle |
|---------|---------|
| **Propósito** | Reporte de ventas completo, métricas, ventas diarias; reportes avanzados (rentabilidad, top productos/categorías, clientes, estaciones, mesas, inventario, tendencias, auditoría, sistema) y exportación PDF/Excel. |
| **Endpoints (Reports)** | `GET /Reports/Index`, `GET /Reports/SalesReport`, `GET /Reports/GetSalesMetrics`, `GET /Reports/GetDailySales`, (y otros similares). |
| **Endpoints (AdvancedReports)** | `GET /AdvancedReports/GetProductProfitability`, `GetCategoryProfitability`, `GetTopSellingProducts`, `GetTopSellingCategories`, `GetTopCustomers`, `GetCustomerSegments`, `GetStationPerformance`, `GetTableUtilization`, `GetInventoryAnalysis`, `GetSupplierAnalysis`, `GetTrendAnalysis`, `GetAuditReport`, `GetSystemHealth`, `ExportToPdf`, `ExportToExcel`. |
| **Roles** | Policy **ReportAccess** (admin, manager, accountant). |
| **Dependencias** | SalesReportService, AdvancedReportsService, filtros por BranchId/CompanyId. |
| **Riesgos** | Rangos de fechas grandes pueden ser pesados; exportación puede tardar o fallar por memoria. |

---

### 2.11 Configuración (empresa, sucursal, sistema)

| Aspecto | Detalle |
|---------|---------|
| **Propósito** | Compañías, sucursales, configuración global (moneda, impuestos, descuentos, horarios, notificaciones, backup), categorías. |
| **Endpoints (Company)** | Index, GetCompanies, Create, Edit, Delete, GetById. |
| **Endpoints (Branch)** | Index, Create, Edit, Delete, GetByCompanyId. |
| **Endpoints (AdvancedSettings)** | Index, SystemSettings, Currency, TaxRate, DiscountPolicy, OperatingHours, NotificationSettings, BackupSettings (CRUD por sección). |
| **Endpoints (Category)** | Ver 2.9. |
| **Roles** | Company, Category, Email: **SystemConfig** (admin). Branch: **admin** (Roles). AdvancedSettings: **[Authorize]** (cualquier autenticado) — posible inconsistencia. |
| **Dependencias** | CompanyService, BranchService, SystemSettingsService, CurrencyService, TaxRateService, DiscountPolicyService, OperatingHoursService, NotificationSettingsService, BackupSettingsService, RestBarContext. |
| **Riesgos** | AdvancedSettings solo exige estar logueado; debería alinearse con SystemConfig o ManagerOrAbove. Cambios en TaxRate/DiscountPolicy afectan cálculos de órdenes/pagos. |

---

### 2.12 Notificaciones

| Aspecto | Detalle |
|---------|---------|
| **Propósito** | SignalR: actualizaciones de órdenes, ítems, mesas, cocina y pagos. Entidad Notification en DB (uso limitado en controladores revisados). |
| **Endpoints** | No hay controlador específico de notificaciones; OrderHubService notifica: OrderStatusChanged, OrderItemStatusChanged, TableStatusChanged, KitchenUpdate, PaymentProcessed, NotifyStationUpdate. |
| **Hub** | `/orderHub` — grupos: order_{id}, table_{id}, table_all, kitchen, station_{stationType}, orders, stock_updates. |
| **Riesgos** | Desconexión de SignalR puede dejar vistas desactualizadas; sin reconexión automática explícita documentada en este mapeo. |

---

### 2.13 Personas (cuentas separadas)

| Aspecto | Detalle |
|---------|---------|
| **Propósito** | Crear personas por orden y listar personas de una orden (split de cuenta). |
| **Endpoints** | `POST /Person/CreatePerson`, `GET /Person/GetPersonsByOrder/{orderId}` (y otros métodos de persona si existen). |
| **Roles** | **Sin [Authorize] a nivel de controlador** — cualquier usuario autenticado (o ruta accesible si el middleware no cubre /Person) puede crear/listar personas. |
| **Dependencias** | PersonService, OrderService. |
| **Riesgos** | **Riesgo de autorización:** PersonController sin política; posible acceso indebido o uso incorrecto si se expone. |

---

### 2.14 Email y plantillas

| Aspecto | Detalle |
|---------|---------|
| **Propósito** | Plantillas de email, prueba SMTP, envío de confirmación de orden (desde PaymentController y OrderController). |
| **Endpoints** | `GET /Email/Index`, `POST /Email/TestConnection`, y otros CRUD de plantillas. |
| **Roles** | **SystemConfig** (admin). |
| **Dependencias** | EmailService, EmailTemplateService, appsettings (Email:Enabled, Smtp). |
| **Riesgos** | Email deshabilitado por defecto; fallos de envío no deben bloquear pago (ya manejado con try/catch). |

---

### 2.15 Logs y auditoría

| Aspecto | Detalle |
|---------|---------|
| **Propósito** | Registro de actividades (AuditMiddleware), errores (ErrorHandlingMiddleware) y vista filtrada de logs. |
| **Endpoints** | `GET /Audit/Index` (filtros: module, action, logLevel, startDate, endDate). |
| **Roles** | **[Authorize]** — cualquier autenticado. No restringido por ReportAccess ni AdminOnly. |
| **Dependencias** | AuditLogService, GlobalLoggingService. |
| **Riesgos** | Filtro por compañía (Branch.CompanyId); usuario sin compañía ve todos o ninguno según implementación. Límite 1000 registros. |

---

### 2.16 Concurrencia y versionado

| Aspecto | Detalle |
|---------|---------|
| **Propósito** | Evitar actualizaciones perdidas en Order mediante token de concurrencia (Order.Version). |
| **Implementación** | Migración AddOrderVersionConcurrencyToken; Version incrementado en PaymentController y OrderService; DbUpdateConcurrencyException devuelta como 409. |
| **Riesgos** | Cualquier actualización de Order sin comprobar Version puede provocar 409 o sobrescritura si no se usa correctamente. |

---

### 2.17 SuperAdmin (multi-tenant)

| Aspecto | Detalle |
|---------|---------|
| **Propósito** | Dashboard global de compañías, sucursales y admins; gestión de compañías/sucursales a nivel sistema. |
| **Endpoints** | `GET /SuperAdmin/Index`, `GET /SuperAdmin/Companies`, (y otros para CRUD compañías/sucursales desde superadmin). |
| **Roles** | **superadmin** únicamente. PermissionMiddleware permite todo a SuperAdmin. |
| **Dependencias** | RestBarContext. |
| **Riesgos** | Acceso total a datos; sin auditoría específica documentada para acciones superadmin. |

---

### 2.18 Seed y utilidades

| Aspecto | Detalle |
|---------|---------|
| **Propósito** | Poblar datos de prueba (compañía, sucursal, áreas, mesas, estaciones, categorías, productos, usuario, orden). Generar hash de contraseña y crear admin. |
| **Endpoints** | `GET/POST /Seed/Seed` (AllowAnonymous), `GET /Seed/GeneratePasswordHash`, `GET /Seed/CreateAdmin` (AllowAnonymous). |
| **Roles** | **AllowAnonymous** — riesgo alto en producción si no se desactiva o restringe. |
| **Riesgos** | **Crítico:** En producción, Seed y CreateAdmin no deberían ser accesibles sin restricción. |

---

## 3. Matriz de módulos

| Módulo | Endpoints principales | Roles | Dependencias | Riesgo técnico |
|--------|----------------------|--------|--------------|-----------------|
| **Autenticación** | /Auth/Login, Logout, Profile, CurrentUser, AccessDenied | Anonymous / Authorize | AuthService, UserService, EmailService | Logs con datos sensibles; sesión larga |
| **Usuarios** | /User/GetUsers, GetUser, Create, Update, Delete | UserManagement (admin, manager, support) | UserService, GlobalLoggingService | Password en form; null Branch |
| **Roles** | (No CRUD; enum en código) | — | — | inventarista en política pero no en enum |
| **Órdenes** | /Order/* (Create, Edit, AddItem, SendToKitchen, Cancel, GetActiveOrder, etc.) | OrderAccess | Order, OrderItem, Table, Product, OrderHub, Email | Concurrencia (Version); null refs |
| **Pagos** | /api/Payment/partial, order/{id}/summary, DELETE void | PaymentAccess | Payment, Order, SplitPayment, OrderHub | Integridad financiera; doble pago; void |
| **Cocina (KDS)** | /Order/StationOrders, MarkItemReady, KitchenOrders | OrderAccess + permiso "kitchen" | OrderService, Station, OrderHub | Productos sin estación no aparecen |
| **Inventario** | /Inventory/GetLowStockProducts, GetInventoryData, ConsumptionReport | InventoryAccess | Product, ProductStockAssignment | inventarista no en enum; filtro tenant |
| **Mesas** | /Table/GetTables, Create, Edit, Delete, ReleaseGhostTables | admin, manager, supervisor | TableService, AreaService | ReleaseGhostTables masivo |
| **Áreas** | /Area/GetAreas, Create, Edit, Delete | admin, manager | AreaService, BranchService | — |
| **Productos** | /Product/GetProducts, Create, Edit, Delete, GetAvailableStock, CheckStockAvailability | ProductAccess | ProductService, Category, Station | AllowNegativeStock; FK categoría |
| **Asignación stock** | /ProductStockAssignment/GetAssignments, Create, Update, Delete | ProductAccess | ProductStockAssignmentService | Ítems sin asignación no en KDS |
| **Estaciones** | /Station/GetStations, Create, Edit, Delete, GetStationTypes | admin, manager | StationService, AreaService | — |
| **Categorías** | /Category/Index, Create, Edit, Delete | SystemConfig | CategoryService, ProductService | Eliminar con productos |
| **Reportes** | /Reports/SalesReport, GetSalesMetrics, GetDailySales | ReportAccess | SalesReportService | Fechas grandes; rendimiento |
| **Reportes avanzados** | /AdvancedReports/* (profitability, top, export PDF/Excel) | ReportAccess | AdvancedReportsService | Export pesado; memoria |
| **Configuración** | /Company/*, /Branch/*, /AdvancedSettings/*, /Category/* | SystemConfig / admin | Varios servicios de configuración | AdvancedSettings solo [Authorize] |
| **Notificaciones** | SignalR /orderHub (grupos order, table, kitchen, station) | (Según contexto de página) | OrderHubService | Desconexión; vistas desactualizadas |
| **Personas** | /Person/CreatePerson, GetPersonsByOrder | **Sin Authorize a nivel controlador** | PersonService, OrderService | **Autorización débil** |
| **Email** | /Email/Index, TestConnection | SystemConfig | EmailService, EmailTemplateService | Email deshabilitado; no bloquear pago |
| **Auditoría** | /Audit/Index | Authorize | AuditLogService | Acceso amplio; límite 1000 |
| **Concurrencia** | (Transacciones en Order/Payment) | — | Order.Version, EF Core | DbUpdateConcurrencyException 409 |
| **SuperAdmin** | /SuperAdmin/Index, Companies, etc. | superadmin | RestBarContext | Acceso total a datos |
| **Seed** | /Seed/Seed, CreateAdmin, GeneratePasswordHash | AllowAnonymous | RestBarContext | **Crítico en producción** |

---

## 4. Dependencias entre módulos (resumen)

- **Órdenes** → Mesas, Productos, Categorías, Áreas, Usuarios, Clientes, SignalR, Email.  
- **Pagos** → Órdenes, SplitPayment, SignalR, Email (confirmación).  
- **Cocina (KDS)** → Órdenes, Estaciones, ProductStockAssignment, SignalR.  
- **Inventario** → Productos, ProductStockAssignment, Áreas/Sucursales.  
- **Usuarios** → Sucursales, Roles; **UserAssignment** → Usuarios, Estaciones, Mesas, Áreas.  
- **Configuración** → Compañía, Sucursal; impuestos/descuentos afectan cálculos de órdenes y pagos.  
- **Auditoría** → Todas las acciones pasan por AuditMiddleware y ErrorHandling.

---

## 5. Riesgos priorizados para E2E

1. **Crítico – Finanzas:** Flujo pago parcial → pago completo → void; totales y saldo pendiente; concurrencia (dos pagos simultáneos).  
2. **Crítico – Seguridad:** Seed/CreateAdmin con AllowAnonymous; PersonController sin autorización explícita.  
3. **Alto – Concurrencia:** Dos usuarios editando la misma orden (Version, 409).  
4. **Alto – Datos:** Rol `inventarista` en política pero no en UserRole.  
5. **Medio – Configuración:** AdvancedSettings con solo [Authorize]; impacto de Tax/Discount en cálculos.  
6. **Medio – Operación:** Productos sin asignación de estación no visibles en KDS; mesas fantasma.  
7. **Bajo – UX:** SignalR desconectado; reportes/export con rangos grandes.

---

## 6. Próximos pasos recomendados

- Corregir **Seed** y **PersonController** (autorización y/o desactivar Seed en producción).  
- Añadir **inventarista** a `UserRole` o quitar de política InventoryAccess.  
- Revisar **AdvancedSettings** (SystemConfig o ManagerOrAbove).  
- Definir casos E2E por flujo: Login → Orden → Cocina → Pago (parcial/completo/void) y por rol (waiter, cashier, admin).  
- Incluir en E2E: concurrencia (dos pestañas pagando la misma orden), SignalR (actualización en tiempo real) y reportes con filtros de fechas.

---

*Documento generado como auditoría funcional previa a pruebas E2E. No sustituye la ejecución de pruebas ni el análisis de requisitos de negocio.*
