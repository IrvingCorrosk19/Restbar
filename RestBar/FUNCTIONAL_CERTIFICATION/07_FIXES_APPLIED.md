# Correcciones Aplicadas — Certificación Funcional

**Fecha:** 2026-07-04

## Resumen

| Severidad | Encontrados | Corregidos | Abiertos |
|-----------|-------------|------------|----------|
| Crítico | 5 | 5 | 0 |
| Alto | 4 | 4 | 0 |
| Medio | 3 | 3 | 0 |

## Correcciones por componente

### Autenticación y roles
- **AuthController:** Redirect chef→`Order/StationOrders?stationType=kitchen`, bartender→`bar`
- **Program.cs:** Política `OrderAccess` incluye `chef` y `bartender`
- **Program.cs:** Rate limiter Development 500 req/min (evita 429 en certificación)
- **SeedController:** Usuario `inventarista@restbar.com`, endpoint `SeedCertificationMultiTenant`

### Permisos y middleware
- **PermissionMiddleware:** Excluir `/Seed`, mapeo `/inventory`, `/audit` → permiso `audit`
- **AuthorizationHelper / _Layout:** URLs Cocina corregidas a `/Order/StationOrders`

### Auditoría
- **AuditController:** Parámetro `action` renombrado a `actionFilter` (evita HTTP 400 por token reservado MVC)
- **AuditController:** ViewModel unificado (`RestBar.ViewModels.AuditLogViewModel`), `GetCurrentUserAsync` implementado con `RestBarContext`
- Eliminada clase duplicada `AuditLogViewModel` en el controlador

### POS y productos
- **OrderController.GetProductsByCategory:** `[FromRoute(Name="id")]` — fix binding Guid vacío
- **OrderController.GetActiveOrder:** Mismo fix de ruta
- **OrderController.GetProductsByCategory:** Filtro multi-tenant por branch/company
- **ProductController.GetProducts:** Filtro estricto branch + company
- **ProductService.GetByCategoryIdAsync:** Filtro branch + activos

### Pagos
- **RestBarContext:** Mapeo `IdempotencyKey` → `idempotency_key`, `BranchId`/`CompanyId` → columnas reales
- **PaymentController.GetOrderPaymentSummary:** Guard IDOR cross-branch (403)
- **SQL:** `apply-security-hardening.sql` — columna `idempotency_key`, índices

### Multi-tenant y datos
- **repair-duplicate-orders.sql:** 7 órdenes duplicadas canceladas, índice `idx_unique_active_order_per_table`
- **Enum PG:** valor `inventarista` en `user_role_enum`

### SignalR
- **OrderHub:** `[Authorize]` en hub

### Suite de pruebas
- **Run-FullCertification.ps1:** Lógica invertida en tests de denegación corregida; fix PowerShell `.Count` en arrays de 1 elemento; iteración de categorías para productos POS

## Archivos modificados (principales)

```
Controllers/AuthController.cs
Controllers/AuditController.cs
Controllers/OrderController.cs
Controllers/PaymentController.cs
Controllers/ProductController.cs
Controllers/SeedController.cs
Helpers/AuthorizationHelper.cs
Hubs/OrderHub.cs
Middleware/PermissionMiddleware.cs
Models/RestBarContext.cs
Program.cs
Services/AuthService.cs
Services/ProductService.cs
Views/Shared/_Layout.cshtml
FUNCTIONAL_CERTIFICATION/scripts/*
```
