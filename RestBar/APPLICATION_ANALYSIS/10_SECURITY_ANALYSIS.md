# 10 — Security Analysis

**Sistema:** RestBar  
**Fecha:** 2026-07-04

---

## 1. Modelo de Seguridad — Resumen

RestBar implementa un modelo de seguridad **multicapa** basado en:

1. Cookie Authentication (stateful)
2. ASP.NET Authorization Policies (12 políticas)
3. PermissionMiddleware (path-based)
4. AuthService.HasPermissionAsync (role-action matrix)
5. Ad-hoc branch/company IDOR checks (API endpoints)

**No implementa:** JWT, OAuth, API keys, MFA, RLS en DB, WAF.

---

## 2. Autenticación

### 2.1 Mecanismo

| Aspecto | Implementación |
|---------|----------------|
| Esquema | Cookie Authentication (`RestBarAuth`) |
| Duración | 8 horas con sliding expiration |
| Cookie flags | HttpOnly=true, SameSite=Lax, SecurePolicy=SameAsRequest |
| Login path | `/Auth/Login` |
| Logout path | `/Auth/Logout` |
| Access denied | `/Auth/AccessDenied` |

### 2.2 Password Handling

| Aspecto | Implementación |
|---------|----------------|
| Algoritmo primario | BCrypt (work factor 12) |
| Legacy support | SHA256 Base64 (migración progresiva en login) |
| Reset token | Almacenado en `PasswordHash` como `RESET_TOKEN:{token}:{expiry}` |
| Default admin | `admin@restbar.com` / `Admin123!` (CreateDefaultAdminAsync) |
| Seed passwords | `123456` (SHA256 en seeder demo) |

### 2.3 Claims Emitidos

| Claim | Tipo | Fuente |
|-------|------|--------|
| NameIdentifier | string | User.Id |
| Name | string | User.FullName |
| Email | string | User.Email |
| Role | string | User.Role.ToString() |
| UserId | string (custom) | User.Id |
| UserRole | string (custom) | User.Role.ToString() |
| BranchId | string (custom) | User.BranchId (opcional) |
| BranchName | string (custom) | Branch.Name |
| CompanyId | string (custom) | Branch.CompanyId |
| CompanyName | string (custom) | Company.Name |

### 2.4 Protecciones de Login

| Control | Implementación |
|---------|----------------|
| Rate limiting | 5 requests/60s/IP en auth endpoints |
| CSRF | ValidateAntiForgeryToken en POST login/logout |
| Inactive users | Rechazados en login |
| Reset token users | Bloqueados de login normal |

---

## 3. Autorización

### 3.1 Políticas ASP.NET (12)

| Política | Roles permitidos |
|----------|-----------------|
| AdminOnly | admin |
| ManagerOrAbove | admin, manager |
| SupervisorOrAbove | admin, manager, supervisor |
| OrderAccess | admin, manager, supervisor, waiter, cashier |
| KitchenAccess | admin, manager, supervisor, chef, bartender |
| PaymentAccess | admin, manager, supervisor, cashier, accountant |
| InventoryAccess | admin, manager, supervisor, accountant, inventarista |
| ProductAccess | admin, manager |
| UserManagement | admin, manager, support |
| ReportAccess | admin, manager, accountant |
| AccountingAccess | admin, manager, accountant |
| SystemConfig | admin |

### 3.2 PermissionMiddleware (Path → Action)

| Path | Action | Roles con acceso |
|------|--------|-----------------|
| /order | orders | admin, manager, supervisor, waiter, cashier, chef*, bartender* |
| /stationorders | kitchen | admin, manager, supervisor, chef, bartender |
| /payment | payments | admin, manager, supervisor, cashier, accountant |
| /table | tables | admin, manager, supervisor, waiter, cashier |
| /product | products | admin, manager |
| /user | users | admin, manager, support |
| /report | reports | admin, manager, accountant |
| /company, /branch, /category, /area, /station | admin_only | admin |
| /superadmin | superadmin_only | superadmin |
| /inventory | (sin mapeo) | Solo política ASP.NET |
| /api/* | (sin mapeo) | Solo política ASP.NET |

### 3.3 SuperAdmin Bypass

El rol `superadmin` omite toda validación en PermissionMiddleware.

---

## 4. Roles y Permisos

### 4.1 Enum UserRole (11 roles)

```
superadmin, admin, manager, supervisor, waiter, cashier,
chef, bartender, accountant, support, inventarista
```

### 4.2 Matriz Rol → Módulos (Dashboard)

Ver documento 06_NAVIGATION_MAP para matriz completa de visibilidad por rol.

### 4.3 Inconsistencias Detectadas

| Hallazgo | Severidad | Detalle |
|----------|-----------|---------|
| PermissionMiddleware no mapea /inventory | Media | Depende solo de política ASP.NET |
| Audit solo [Authorize] | Media | Cualquier autenticado ve logs |
| chef/bartender acceden "orders" pero no "tables" | Baja | Pueden ver POS pero no gestionar mesas |
| Dual validación MVC vs API | Media | Rutas /api/* omiten PermissionMiddleware |

---

## 5. Protección de Rutas

### 5.1 Rutas Públicas

| Ruta | Riesgo |
|------|--------|
| /Auth/Login | ✅ Normal |
| /Auth/ForgotPassword | ✅ Normal |
| /Seed/SeedDemoData | ⚠ Bloqueado en prod |
| /Seed/CreateAdminUser | 🔴 AllowAnonymous, sin guard de prod |
| /Auth/CreateAdmin | 🔴 AllowAnonymous, sin guard de prod |

### 5.2 Rutas con Protección Adecuada

La mayoría de rutas operacionales tienen doble capa (policy + middleware).

### 5.3 SignalR Hub

| Aspecto | Estado |
|---------|--------|
| Autenticación en Hub | ❌ Sin [Authorize] |
| Validación de group membership | ❌ Cualquier cliente puede join cualquier group |
| Transport security | Depende de HTTPS en nginx |

---

## 6. Protección contra Accesos Indebidos

### 6.1 IDOR (Insecure Direct Object Reference)

| Endpoint | Protección |
|----------|-----------|
| POST /api/Payment/partial | ✅ Verifica order.BranchId == user.BranchId |
| PersonController | ✅ Verifica branch en CreatePerson |
| InventoryController | ✅ Filtro por BranchId/CompanyId |
| OrderController (general) | ⚠ Depende de servicios (filtrado por branch) |
| SuperAdmin | ⚠ Acceso total cross-tenant (by design) |

### 6.2 CSRF

| Protegido | No protegido |
|-----------|-------------|
| Login, Logout, Reset password | Mayoría de endpoints JSON/AJAX |
| Order mutations (POST) | GET endpoints |
| Category, SuperAdmin forms | API endpoints (/api/*) |

**Nota:** Los endpoints JSON usan cookie auth sin anti-forgery token. Mitigado parcialmente por SameSite=Lax.

### 6.3 Rate Limiting

Solo en auth endpoints (5/min/IP). No hay rate limiting en endpoints operacionales ni API.

---

## 7. Seguridad de Datos

| Aspecto | Estado |
|---------|--------|
| Passwords en DB | BCrypt hashed |
| Connection string en prod | Via env var (no en appsettings.Production.json) |
| Connection string en dev | ⚠ Password en appsettings.Development.json |
| Secrets en deploy scripts | 🔴 SSH password hardcoded en deploy-restbar.ps1 |
| Audit log de passwords | Enmascarados en AuditMiddleware para /api/auth |
| Error messages | ⚠ Exponen exception.Message al cliente |
| SQL injection | Mitigado por EF Core parameterized queries |
| XSS | Razor auto-encoding; JS innerHTML limitado |

---

## 8. Seguridad de Infraestructura

| Control | Estado |
|---------|--------|
| HTTPS | ✅ nginx SSL (Let's Encrypt) en producción |
| HSTS | ✅ Habilitado en non-Development |
| PostgreSQL expuesto | ❌ No expuesto externamente (Docker network) |
| Docker network isolation | ✅ restbar_net bridge |
| Forwarded headers | ✅ ASPNETCORE_FORWARDEDHEADERS_ENABLED en VPS |
| DataProtection keys | ✅ Volumen Docker persistente |
| Cookie SecurePolicy | ⚠ SameAsRequest (permite HTTP) |

---

## 9. Auditoría de Seguridad

| Evento | Registrado en |
|--------|--------------|
| Login/Logout | audit_logs (via AuditMiddleware) |
| Requests HTTP | audit_logs (REQUEST_START/SUCCESS) |
| Errores | audit_logs (via ErrorHandlingMiddleware) |
| Cancelación órdenes | order_cancellation_logs |
| Acciones CRUD | Parcial (via GlobalLoggingService en algunos servicios) |
| Acciones SuperAdmin | ⚠ Sin auditoría específica |
| SignalR connections | ❌ No auditadas |

---

## 10. Hardening Reciente (Migración SecurityHardening)

| Medida | Implementación |
|--------|----------------|
| Idempotency key en pagos | `payments.idempotency_key` + partial unique index |
| Una orden activa por mesa | `idx_unique_active_order_per_table` |
| Rol inventarista | Agregado a enum PG y C# |

---

## 11. Hallazgos de Seguridad Priorizados

| # | Severidad | Hallazgo | Ubicación |
|---|-----------|----------|-----------|
| S1 | 🔴 Crítica | SignalR hub sin autenticación ni validación de groups | Hubs/OrderHub.cs |
| S2 | 🔴 Crítica | CreateAdmin/CreateAdminUser AllowAnonymous en producción | AuthController, SeedController |
| S3 | 🔴 Crítica | SSH credentials en repo | Com/deploy-restbar.ps1 |
| S4 | 🟠 Alta | Password reset token en columna PasswordHash | AuthService |
| S5 | 🟠 Alta | Error responses exponen exception.Message | ErrorHandlingMiddleware |
| S6 | 🟠 Alta | Cookie SecurePolicy=SameAsRequest | Program.cs |
| S7 | 🟡 Media | Audit accesible por cualquier autenticado | AuditController |
| S8 | 🟡 Media | API routes sin PermissionMiddleware | PermissionMiddleware |
| S9 | 🟡 Media | Sin rate limiting en endpoints operacionales | Program.cs |
| S10 | 🟡 Media | DB password en appsettings.Development.json | Config |
| S11 | 🟢 Baja | GenerateJwtTokenAsync naming misleading | AuthService |
| S12 | 🟢 Baja | Session registrada pero sin uso | Program.cs |

---

## 12. Recomendaciones para Certificación (No implementadas)

Estas son observaciones para la fase de certificación, NO acciones tomadas:

1. Validar que Seed/CreateAdmin estén bloqueados en producción
2. Probar IDOR cross-branch en todos los endpoints JSON
3. Verificar que SuperAdmin no pueda ser impersonado
4. Probar brute force en login (rate limiter)
5. Verificar que SignalR no exponga datos de otras sucursales
6. Validar CSRF en formularios POST
7. Verificar que audit logs no contengan datos sensibles

---

*Análisis de seguridad completo. Sin modificaciones al sistema.*
