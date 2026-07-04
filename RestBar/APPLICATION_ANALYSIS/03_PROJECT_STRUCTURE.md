# 03 — Project Structure

**Sistema:** RestBar  
**Fecha:** 2026-07-04

---

## 1. Estructura de la Solución

```
RestBar/                          ← Raíz del repositorio Git
├── RestBar.sln                   ← Solución Visual Studio (1 proyecto)
└── RestBar/                      ← Proyecto web principal
    ├── RestBar.csproj
    ├── Program.cs                ← Entry point, DI, pipeline
    ├── Dockerfile
    ├── docker-compose.yml
    ├── appsettings*.json
    ├── Controllers/              ← 26 controladores
    ├── Services/                 ← 36 servicios + 2 interfaces locales
    ├── Interfaces/               ← 34 interfaces de servicio
    ├── Models/                   ← 35 entidades/enums + DbContext
    ├── ViewModel/                ← 5 DTOs/ViewModels
    ├── ViewModels/               ← 3 ViewModels de reportes
    ├── Views/                    ← 65 vistas Razor (24 carpetas)
    ├── wwwroot/                  ← Assets estáticos
    ├── Middleware/               ← 2 archivos, 3 middlewares
    ├── Hubs/                     ← OrderHub (SignalR)
    ├── Helpers/                  ← AuthorizationHelper, LoggingHelper
    ├── Migrations/               ← 5 migraciones EF Core
    ├── Scripts/                  ← SQL seed (parcialmente desactualizado)
    ├── docs/                     ← Documentación funcional previa
    ├── Com/                      ← Scripts de deploy VPS, nginx, Docker
    └── APPLICATION_ANALYSIS/       ← Este análisis
```

**Tipo de solución:** Proyecto único (no hay capas separadas en proyectos distintos).

---

## 2. Responsabilidades por Carpeta

### Controllers/ (26 archivos)

Capa de entrada HTTP. Responsable de:
- Recibir requests HTTP (MVC y API)
- Aplicar `[Authorize]` y políticas
- Delegar a servicios
- Retornar Views o JSON

| Grupo | Controladores |
|-------|--------------|
| **Autenticación** | AuthController |
| **Dashboard** | HomeController |
| **Operación POS** | OrderController, PersonController |
| **Pagos** | PaymentController (API), PaymentViewController |
| **Cocina** | KitchenApiController |
| **Catálogos** | ProductController, CategoryController, StationController |
| **Layout físico** | TableController, AreaController |
| **Inventario** | InventoryController, ProductStockAssignmentController |
| **Multi-tenant** | CompanyController, BranchController, SuperAdminController |
| **Usuarios** | UserController, UserManagementController, UserAssignmentController |
| **Reportes** | ReportsController, AdvancedReportsController |
| **Configuración** | AdvancedSettingsController, EmailController |
| **Auditoría** | AuditController |
| **Utilidades** | SeedController |

### Services/ (40 archivos)

Capa de lógica de negocio. Todos registrados como `Scoped`.

| Grupo | Servicios |
|-------|-----------|
| **Core POS** | OrderService, OrderItemService, KitchenService, PaymentService, SplitPaymentService, PersonService |
| **Catálogos** | ProductService, CategoryService, ProductCategoryService, ModifierService, StationService |
| **Layout** | TableService, AreaService |
| **Inventario** | ProductStockAssignmentService |
| **Multi-tenant** | CompanyService, BranchService, CustomerService |
| **Usuarios** | UserService, UserAssignmentService, AuthService |
| **Finanzas** | InvoiceService |
| **Reportes** | SalesReportService, AdvancedReportsService |
| **Configuración** | SystemSettingsService, CurrencyService, TaxRateService, DiscountPolicyService, OperatingHoursService, NotificationSettingsService, BackupSettingsService |
| **Comunicación** | EmailService, EmailTemplateService, NotificationService, OrderHubService |
| **Infraestructura** | AuditLogService, GlobalLoggingService |
| **Base** | BaseTrackingService (abstract) |

### Interfaces/ (34 archivos)

Contratos de servicio. Convención: `I{Name}Service.cs`.

**Excepciones** (interfaces fuera de carpeta):
- `Services/IKitchenService.cs`
- `Services/IOrderHubService.cs`

### Models/ (36 archivos)

| Tipo | Archivos |
|------|----------|
| **DbContext** | RestBarContext.cs |
| **Entidades tenant** | Company, Branch, Area, Table, Station |
| **Entidades operación** | Order, OrderItem, Payment, SplitPayment, Person, Customer, Invoice |
| **Entidades catálogo** | Product, Category, ProductCategory, Modifier, ProductStockAssignment |
| **Entidades sistema** | User, UserAssignment, AuditLog, Notification, OrderCancellationLog |
| **Entidades configuración** | SystemSettings, Currency, TaxRate, DiscountPolicy, OperatingHours, NotificationSettings, BackupSettings, EmailTemplate |
| **Enums** | UserRole (en User.cs), OrderStatus, OrderType, OrderItemStatus, TableStatus |
| **Interfaces** | ITrackableEntity |
| **View helpers** | ErrorViewModel |

### Views/ (24 carpetas + Shared)

Organización por convención MVC: una carpeta por controlador.

| Layout | Uso |
|--------|-----|
| `_Layout.cshtml` | Admin/general (navbar completo) |
| `_OrderLayout.cshtml` | Pantalla POS (mínimo chrome) |
| `_KitchenLayout.cshtml` | KDS pantalla completa |
| `_LoginLayout.cshtml` | Login y AccessDenied |

### wwwroot/

| Carpeta | Contenido |
|---------|-----------|
| `css/` | 9 hojas de estilo (site, order, inventory, kitchen, etc.) |
| `js/` | 28 módulos JS (order/, inventory/, advanced-reports/, supplier/) |
| `lib/` | Bootstrap, jQuery, jQuery Validation (vendor) |
| `images/` | Referenciado pero vacío en repo |

### Middleware/ (2 archivos)

| Archivo | Clases |
|---------|--------|
| `AuditMiddleware.cs` | AuditMiddleware, ErrorHandlingMiddleware + extensions |
| `PermissionMiddleware.cs` | PermissionMiddleware + extensions |

### Hubs/ (1 archivo)

`OrderHub.cs` — SignalR hub para tiempo real.

### Helpers/ (2 archivos)

| Archivo | Propósito |
|---------|-----------|
| `AuthorizationHelper.cs` | Helpers Razor para verificación de roles |
| `LoggingHelper.cs` | Logging unificado Console + ILogger |

### Migrations/ (5 + snapshot)

| Migración | Fecha | Cambio principal |
|-----------|-------|-----------------|
| InitialCreate | 2025-11-02 | Schema completo |
| AddAllowNegativeStockToProducts | 2025-11-03 | Stock e inventario |
| RemoveStationIdFromProducts | 2025-11-08 | Desacople producto-estación |
| AddOrderVersionConcurrencyToken | 2026-02-26 | Concurrencia en órdenes |
| SecurityHardening | 2026-02-27 | Idempotencia pagos, orden única por mesa |

### Com/ (deployment)

Scripts PowerShell y configuraciones para VPS multi-app:

| Subcarpeta/Archivo | Propósito |
|-------------------|-----------|
| `restbar/docker-compose.yml` | Stack Docker producción |
| `restbar/nginx-*.conf` | Configuración nginx SSL |
| `deploy-restbar.ps1` | Deploy remoto VPS |
| `CONFIGURACION_RESTBAR_VPS.md` | Documentación VPS |
| `Documentacion/` | Docs genéricos (mayormente CarnetQR) |

---

## 3. Dependencias entre Proyectos

```
RestBar.csproj (único proyecto)
  ├── Microsoft.EntityFrameworkCore 9.0.5
  ├── Npgsql.EntityFrameworkCore.PostgreSQL 9.0.4
  ├── Microsoft.AspNetCore.SignalR 1.1.0
  ├── BCrypt.Net-Next 4.0.3
  ├── MailKit 4.14.1
  └── MimeKit 4.14.0
```

No hay referencias a otros proyectos .NET. No hay proyectos de tests en la solución.

---

## 4. Convenciones de Nomenclatura

| Elemento | Convención | Ejemplo |
|----------|-----------|---------|
| Controladores | `{Name}Controller` | OrderController |
| Servicios | `{Name}Service` | OrderService |
| Interfaces | `I{Name}Service` | IOrderService |
| Vistas | `{Action}.cshtml` en `Views/{Controller}/` | Views/Order/Index.cshtml |
| Entidades | PascalCase singular | Order, OrderItem |
| Tablas DB | snake_case (mayoría) | orders, order_items |
| Tablas config | PascalCase | SystemSettings, Currencies |
| Enums | PascalCase | OrderStatus, UserRole |
| JS modules | kebab-case | order-management.js |
| SignalR groups | snake con prefijo | order_{id}, station_{type} |

---

## 5. Archivos Especiales

| Archivo | Propósito |
|---------|-----------|
| `Program.cs` | Bootstrap, DI, middleware, migraciones auto |
| `Dockerfile` | Build multi-stage para producción |
| `docker-compose.yml` | Orquestación web + postgres |
| `.dockerignore` | Exclusiones de build |
| `Services/OrderService.cs.backup` | Backup obsoleto (no registrado en DI) |
| `Scripts/SeedDummyData.sql` | Seed SQL (desactualizado: referencia station_id eliminado) |

---

## 6. Organización del Frontend

El frontend NO es un SPA separado. Es **server-rendered MVC** con enriquecimiento JavaScript:

```
Razor View (HTML inicial)
  + Partials (_Categories, _Products, _OrderSummary...)
  + Layout específico (_OrderLayout para POS)
  + Módulos JS cargados por vista
  + AJAX fetch a endpoints JSON del mismo controller
  + SignalR para actualizaciones real-time
```

---

## 7. Mapa de Responsabilidades (Quién hace qué)

| Responsabilidad | Ubicación |
|----------------|-----------|
| Routing HTTP | Controllers + Program.cs |
| Validación de entrada | Controllers (ModelState) + Data Annotations |
| Reglas de negocio | Services |
| Acceso a datos | Services → RestBarContext |
| Autenticación | AuthService + Cookie middleware |
| Autorización (políticas) | Program.cs policies + [Authorize] |
| Autorización (rutas) | PermissionMiddleware + AuthService |
| Auditoría de requests | AuditMiddleware |
| Manejo de errores | ErrorHandlingMiddleware + try/catch en controllers |
| Tiempo real | OrderHub + OrderHubService |
| Email | EmailService (MailKit) |
| Configuración | appsettings.json + SystemSettings (DB) |
| Multi-tenant filtering | BaseTrackingService + HttpContextAccessor |
| Tracking Created/Updated | RestBarContext.ApplyTrackingChanges() |

---

*Documento de estructura de proyecto. Sin modificaciones al sistema.*
