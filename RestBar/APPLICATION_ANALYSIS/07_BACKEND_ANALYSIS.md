# 07 — Backend Analysis

**Sistema:** RestBar  
**Fecha:** 2026-07-04

---

## 1. Inventario de Controladores (26)

### 1.1 Controladores MVC (24)

| Controller | Hereda | Auth | Acciones principales | Retorno |
|-----------|--------|------|---------------------|---------|
| AuthController | Controller | Mixed | Login, Logout, Profile, CurrentUser, ForgotPassword, ResetPassword, CreateAdmin | View + JSON |
| HomeController | Controller | Authorize | Index, Privacy, Error | View |
| OrderController | Controller | OrderAccess | 30+ acciones CRUD + AJAX | View + JSON |
| PaymentViewController | Controller | PaymentAccess | Index, DashboardStats, RecentPayments, Analytics | View + JSON |
| PersonController | Controller | OrderAccess | CreatePerson, GetPersonsByOrder, AssignItem | JSON only |
| TableController | Controller | Roles | CRUD + ReleaseGhostTables, FixTableStatus | View + JSON |
| AreaController | Controller | Roles | CRUD + Ajax | View + JSON |
| ProductController | Controller | ProductAccess | CRUD + stock checks | View + JSON |
| ProductStockAssignmentController | Controller | ProductAccess | CRUD assignments | View + JSON |
| StationController | Controller | Roles | CRUD + Ajax | View + JSON |
| CategoryController | Controller | SystemConfig | CRUD + EditAjax | View + JSON |
| InventoryController | Controller | InventoryAccess | Index, GetLowStock, ConsumptionReport | View + JSON |
| CompanyController | Controller | SystemConfig | CRUD | View + JSON |
| BranchController | Controller | admin | CRUD + REST endpoints | View + JSON |
| UserController | Controller | UserManagement | CRUD + UserManagement view | View + JSON |
| UserManagementController | Controller | UserManagement | Index, Create | View + JSON |
| UserAssignmentController | Controller | UserManagement | CRUD assignments | View + JSON |
| ReportsController | Controller | ReportAccess | SalesReport, metrics, export | View + JSON |
| AdvancedReportsController | Controller | ReportAccess | 10 report types + export | View + JSON |
| AdvancedSettingsController | Controller | ManagerOrAbove | Settings CRUD + ExecuteBackup | View + JSON |
| AuditController | Controller | Authorize | Index, Details, Export | View + JSON |
| EmailController | Controller | SystemConfig | TestConnection, templates CRUD | JSON |
| SuperAdminController | Controller | superadmin | Companies, Branches, Admins CRUD | View + JSON |
| SeedController | Controller | Mixed | SeedDemoData, CreateAdminUser, GeneratePasswordHash | View + JSON |

### 1.2 Controladores API (2)

| Controller | Hereda | Route | Auth |
|-----------|--------|-------|------|
| PaymentController | ControllerBase | `api/Payment` | PaymentAccess |
| KitchenApiController | ControllerBase | `api/kitchen` | KitchenAccess |

---

## 2. Inventario de Servicios (36 implementaciones)

### 2.1 Servicios Core POS

| Servicio | Interface | Dependencias clave |
|----------|-----------|-------------------|
| OrderService | IOrderService | RestBarContext, OrderItemService, ProductService, TableService, OrderHubService, EmailService |
| OrderItemService | IOrderItemService | RestBarContext |
| KitchenService | IKitchenService | RestBarContext, OrderHubService |
| PaymentService | IPaymentService | RestBarContext, HttpContextAccessor, OrderHubService, EmailService |
| SplitPaymentService | ISplitPaymentService | RestBarContext |
| PersonService | IPersonService | RestBarContext, HttpContextAccessor |

### 2.2 Servicios de Catálogo

| Servicio | Interface | Extiende BaseTrackingService |
|----------|-----------|------------------------------|
| ProductService | IProductService | Sí |
| CategoryService | ICategoryService | Sí |
| ProductCategoryService | IProductCategoryService | No |
| ModifierService | IModifierService | No |
| StationService | IStationService | Sí |
| ProductStockAssignmentService | IProductStockAssignmentService | Sí |

### 2.3 Servicios de Layout

| Servicio | Interface |
|----------|-----------|
| TableService | ITableService |
| AreaService | IAreaService |

### 2.4 Servicios Multi-Tenant

| Servicio | Interface |
|----------|-----------|
| CompanyService | ICompanyService |
| BranchService | IBranchService |
| CustomerService | ICustomerService |

### 2.5 Servicios de Usuarios

| Servicio | Interface |
|----------|-----------|
| UserService | IUserService |
| UserAssignmentService | IUserAssignmentService |
| AuthService | IAuthService |

### 2.6 Servicios de Reportes

| Servicio | Interface |
|----------|-----------|
| SalesReportService | ISalesReportService |
| AdvancedReportsService | IAdvancedReportsService |

### 2.7 Servicios de Configuración

| Servicio | Interface |
|----------|-----------|
| SystemSettingsService | ISystemSettingsService |
| CurrencyService | ICurrencyService |
| TaxRateService | ITaxRateService |
| DiscountPolicyService | IDiscountPolicyService |
| OperatingHoursService | IOperatingHoursService |
| NotificationSettingsService | INotificationSettingsService |
| BackupSettingsService | IBackupSettingsService |

### 2.8 Servicios de Infraestructura

| Servicio | Interface | Propósito |
|----------|-----------|-----------|
| EmailService | IEmailService | SMTP via MailKit |
| EmailTemplateService | IEmailTemplateService | CRUD plantillas |
| NotificationService | INotificationService | Notificaciones in-app |
| AuditLogService | IAuditLogService | Persistir audit logs |
| GlobalLoggingService | IGlobalLoggingService | Wrapper de logging |
| OrderHubService | IOrderHubService | Push SignalR |
| InvoiceService | IInvoiceService | Facturación |

### 2.9 Clase Base

```csharp
BaseTrackingService (abstract)
  ├── SetTrackingFields() — CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
  ├── GetCurrentUserId() — desde claims
  ├── GetCurrentBranchId() — desde claims
  └── GetCurrentCompanyId() — desde claims
```

---

## 3. DTOs y ViewModels

| Archivo | Ubicación | Uso |
|---------|-----------|-----|
| PaymentDto | ViewModel/PaymentDto.cs | API pagos |
| SendOrderDto | ViewModel/SendOrderDto.cs | Envío a cocina |
| KitchenOrderViewModel | ViewModel/KitchenOrderViewModel.cs | KDS display |
| ProductCreateViewModel | ViewModel/ProductCreateViewModel.cs | Form crear producto |
| ProductEditViewModel | ViewModel/ProductEditViewModel.cs | Form editar producto |
| AdvancedReportsViewModels | ViewModels/AdvancedReportsViewModels.cs | Filtros de reportes |
| SalesReportViewModel | ViewModels/SalesReportViewModel.cs | Reporte ventas |
| AuditLogViewModel | ViewModels/AuditLogViewModel.cs | Vista auditoría |
| DashboardViewModel | HomeController.cs (inline) | Dashboard cards |
| CardVisibility | HomeController.cs (inline) | Visibilidad por rol |

---

## 4. Middleware

### 4.1 AuditMiddleware

| Aspecto | Detalle |
|---------|---------|
| **Ubicación** | `Middleware/AuditMiddleware.cs` |
| **Trigger** | Cada request post-autenticación |
| **Acciones** | REQUEST_START al inicio, REQUEST_SUCCESS al completar |
| **Excepciones** | Re-throw para ErrorHandlingMiddleware |
| **Datos capturados** | Path, Method, UserId, IP, UserAgent, Duration |
| **Enmascaramiento** | Password/token en rutas /api/auth, /api/user |

### 4.2 ErrorHandlingMiddleware

| Aspecto | Detalle |
|---------|---------|
| **Ubicación** | `Middleware/AuditMiddleware.cs` (mismo archivo) |
| **Mapeo excepciones** | UnauthorizedAccessException→401, InvalidOperationException→400, KeyNotFoundException→404, else→500 |
| **Respuesta** | JSON `{ Error, StatusCode, Timestamp, Path, Method }` |
| **Logging** | AuditLogService.LogErrorAsync() |
| **Nota** | Expone `exception.Message` al cliente |

### 4.3 PermissionMiddleware

| Aspecto | Detalle |
|---------|---------|
| **Ubicación** | `Middleware/PermissionMiddleware.cs` |
| **Rutas excluidas** | /Auth, /Home/Error, /, estáticos |
| **No autenticado** | Redirect → /Auth/Login |
| **SuperAdmin** | Bypass total |
| **Sin permiso** | Redirect → /Auth/AccessDenied |
| **Mapeo** | Path prefix → action string → AuthService.HasPermissionAsync |

---

## 5. Autenticación y Autorización

### 5.1 AuthService — Métodos Clave

| Método | Propósito |
|--------|-----------|
| LoginAsync | Validar credenciales, crear cookie |
| LogoutAsync | Eliminar sesión |
| HasPermissionAsync | Verificar action por userId |
| GetCurrentUserAsync | Obtener usuario actual |
| CreateDefaultAdminAsync | Crear admin por defecto |
| ForgotPasswordAsync | Generar token reset |
| ResetPasswordAsync | Validar token y actualizar password |
| HashPassword / VerifyPassword | BCrypt + legacy SHA256 |

### 5.2 Matriz de Permisos (HasPermissionAsync)

| Rol | Acciones permitidas |
|-----|-------------------|
| superadmin | Todas |
| admin | Todas excepto superadmin_only |
| manager | Todas excepto admin_only, superadmin_only |
| supervisor | orders, kitchen, payments, tables |
| waiter | orders, tables, customers |
| cashier | orders, payments, customers |
| chef | kitchen, orders |
| bartender | orders, kitchen |
| accountant | payments, reports |
| support | orders, users |
| inventarista | inventory, reports |

---

## 6. Dependency Injection

**Patrón:** Todos los servicios registrados como `Scoped`.

**Factory registrations** (inyección explícita de HttpContextAccessor):
- StationService, AreaService, BranchService, UserService, CustomerService
- PaymentService, InvoiceService, NotificationService, AuthService
- ProductStockAssignmentService

**DbContext factory:**
```csharp
AddScoped<RestBarContext>(provider => {
    var context = new RestBarContext(options);
    context.HttpContextAccessor = httpContextAccessor;
    return context;
});
```

---

## 7. Validaciones

| Mecanismo | Uso |
|-----------|-----|
| Data Annotations | `[Required]`, `[StringLength]` en entidades y DTOs |
| ModelState.IsValid | Form POST en Category, Station, SuperAdmin, UserManagement |
| `[ValidateAntiForgeryToken]` | Login, Logout, Reset, Order mutations, Category, SuperAdmin |
| Validación manual | Payment API (amount > 0, branch IDOR), PersonController |
| Reglas de negocio | InvalidOperationException en servicios |
| Rate Limiting | Auth endpoints (5/min/IP) |

**No utilizado:** FluentValidation, IValidatableObject, ProblemDetails.

---

## 8. Manejo de Excepciones

| Capa | Estrategia |
|------|-----------|
| Development | UseDeveloperExceptionPage (HTML stack trace) |
| Production MVC | UseExceptionHandler → Home/Error (HTML) |
| Middleware | ErrorHandlingMiddleware → JSON |
| Controllers | try/catch → Json({ success: false, message }) |
| Services | throw typed exceptions |
| Concurrencia | DbUpdateConcurrencyException → HTTP 409 |

---

## 9. Caching y Sesión

| Componente | Estado |
|-----------|--------|
| DistributedMemoryCache | Registrado (para Session) |
| Session (30 min idle) | Registrado pero **sin uso** en código |
| Response caching | Deshabilitado en dev (no-cache headers) |
| EF Core query caching | Default (implícito) |
| Redis / distributed cache | No implementado |

---

## 10. Logging

| Componente | Destino |
|-----------|---------|
| ILogger<T> | ASP.NET Core default (Console) |
| GlobalLoggingService | Console + ILogger wrapper |
| AuditLogService | PostgreSQL `audit_logs` table |
| AuditMiddleware | AuditLogService (cada request) |
| ErrorHandlingMiddleware | AuditLogService (errores) |

**No utilizado:** Serilog, NLog, Application Insights, structured logging.

---

## 11. Background Services / Jobs

**Resultado: NINGUNO.**

| Búsqueda | Resultado |
|----------|----------|
| IHostedService | No encontrado |
| BackgroundService | No encontrado |
| Hangfire | No instalado |
| Quartz | No instalado |
| Timer/scheduled tasks | No encontrado |

`BackupSettingsService.ExecuteBackupAsync()` simula backup con `Task.Delay(2000)` — solo ejecutable manualmente desde AdvancedSettingsController.

---

## 12. Eventos

No hay sistema de eventos de dominio. La comunicación entre módulos es:

1. **Llamadas directas** entre servicios (DI)
2. **SignalR** para notificaciones real-time (OrderHubService)
3. **AuditLog** para trazabilidad post-facto

---

## 13. Helpers

| Helper | Propósito |
|--------|-----------|
| AuthorizationHelper | Métodos estáticos para verificar roles en Razor |
| LoggingHelper | Console.WriteLine + ILogger unificado |

---

## 14. Hubs (SignalR)

### OrderHub

| Método (cliente invoca) | Propósito |
|------------------------|-----------|
| JoinOrderGroup(orderId) | Suscribirse a orden |
| LeaveOrderGroup(orderId) | Desuscribirse |
| JoinTableGroup(tableId) | Suscribirse a mesa |
| JoinAllTablesGroup() | Todas las mesas |
| JoinKitchenGroup() | Grupo cocina |
| JoinStationTypeGroup(type) | Por tipo de estación |
| JoinOrdersGroup() | Todas las órdenes |

**⚠ Sin [Authorize] en el Hub ni validación de membresía.**

---

*Análisis de backend completo. Sin modificaciones al sistema.*
