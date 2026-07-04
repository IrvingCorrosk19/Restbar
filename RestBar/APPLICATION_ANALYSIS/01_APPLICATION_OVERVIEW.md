# 01 — Application Overview

**Sistema:** RestBar  
**Tipo:** POS / SaaS Multi-Tenant para restaurantes y bares  
**Fecha de análisis:** 2026-07-04  
**Alcance:** Análisis funcional y técnico completo (sin modificaciones)  
**Propósito:** Base documental para Certificación Funcional Enterprise

---

## 1. Identidad del Sistema

RestBar es una aplicación web monolítica orientada a la operación integral de restaurantes y bares. Cubre el ciclo completo desde la toma de pedidos en mesa, envío a cocina/bar (KDS), gestión de inventario, procesamiento de pagos (incluyendo cuentas separadas y pagos parciales), hasta reportes financieros y administración multi-sucursal.

El sistema está diseñado como **SaaS multi-tenant** con jerarquía `Company → Branch`, permitiendo que un operador (SuperAdmin) gestione múltiples empresas, cada una con una o más sucursales independientes.

---

## 2. Stack Tecnológico

| Capa | Tecnología | Versión |
|------|-----------|---------|
| Runtime | .NET | 8.0 |
| Framework web | ASP.NET Core MVC + API Controllers | 8.0 |
| ORM | Entity Framework Core | 9.0.5 |
| Base de datos | PostgreSQL (Npgsql) | 15 (Docker) / Provider 9.0.4 |
| Autenticación | Cookie Authentication | ASP.NET Core Identity-less |
| Tiempo real | SignalR | 1.1.0 |
| Email | MailKit + MimeKit | 4.14.x |
| Hashing | BCrypt.Net-Next | 4.0.3 |
| Frontend | Razor Views + jQuery + Bootstrap 5 | — |
| Contenedores | Docker + nginx reverse proxy | ASP.NET 8 runtime |
| Cultura | es-PA (Panamá) | dd/MM/yyyy |

---

## 3. Arquitectura de Alto Nivel

```
┌─────────────────────────────────────────────────────────────────┐
│                        CLIENTE (Browser)                        │
│  Razor Views │ jQuery/AJAX │ SignalR Client │ Bootstrap 5       │
└────────────────────────────┬────────────────────────────────────┘
                             │ HTTPS + Cookies
┌────────────────────────────▼────────────────────────────────────┐
│                    ASP.NET Core (RestBar)                       │
│  ┌─────────────┐  ┌──────────────┐  ┌─────────────────────┐  │
│  │ Controllers │→ │   Services   │→ │  RestBarContext     │  │
│  │  (26 total) │  │  (36 scoped) │  │  (EF Core + PG)     │  │
│  └─────────────┘  └──────────────┘  └─────────────────────┘  │
│  ┌─────────────┐  ┌──────────────┐  ┌─────────────────────┐  │
│  │ Middleware  │  │  OrderHub    │  │  Auth (Cookies)     │  │
│  │ Audit/Error │  │  (SignalR)   │  │  + Policies         │  │
│  │ Permission  │  └──────────────┘  └─────────────────────┘  │
│  └─────────────┘                                               │
└────────────────────────────┬────────────────────────────────────┘
                             │
┌────────────────────────────▼────────────────────────────────────┐
│              PostgreSQL 15 (restbar_postgres)                   │
│  29 entidades │ 5 migraciones │ Multi-tenant por Company/Branch  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 4. Tipo de Solución

| Aspecto | Detalle |
|---------|---------|
| **Patrón arquitectónico** | Monolito MVC con capa de servicios (Service Layer) |
| **Proyectos en solución** | 1 proyecto web (`RestBar.csproj`) |
| **Separación de capas** | Controllers → Services → DbContext (sin repositorios explícitos) |
| **Patrón de datos** | Active Record via EF Core; `BaseTrackingService` para auditoría de campos |
| **API vs MVC** | Híbrido: 24 controladores MVC + 2 API (`PaymentController`, `KitchenApiController`) |
| **Multi-tenancy** | Aplicación (filtros por `CompanyId`/`BranchId` en servicios) — sin RLS en DB |

---

## 5. Dominios Funcionales Identificados

| # | Módulo | Estado |
|---|--------|--------|
| 1 | Autenticación y sesión | Implementado |
| 2 | Dashboard por rol | Implementado |
| 3 | POS / Órdenes | Implementado (núcleo) |
| 4 | Cocina / KDS (Kitchen Display) | Implementado |
| 5 | Pagos y cuentas separadas | Implementado |
| 6 | Mesas y áreas | Implementado |
| 7 | Productos y categorías | Implementado |
| 8 | Estaciones (cocina/bar) | Implementado |
| 9 | Inventario y stock por estación | Implementado |
| 10 | Usuarios y asignaciones | Implementado |
| 11 | Multi-tenant (Company/Branch) | Implementado |
| 12 | Reportes básicos y avanzados | Implementado |
| 13 | Configuración avanzada | Parcialmente implementado |
| 14 | Email y plantillas | Implementado (deshabilitado por defecto) |
| 15 | Auditoría y logs | Implementado |
| 16 | SuperAdmin (gestión global) | Implementado |
| 17 | Clientes / CRM básico | Parcial (entidad existe, UI limitada) |
| 18 | Facturación | Parcial (entidad `Invoice`, sin flujo completo UI) |
| 19 | Modificadores de producto | Parcial (entidad existe, UI limitada) |
| 20 | Proveedores | No implementado (JS huérfano) |
| 21 | Compras | No implementado |
| 22 | Promociones / descuentos | Parcial (políticas en config, modal en POS) |
| 23 | Impresiones / impresoras | No implementado (vista referenciada sin implementar) |
| 24 | Backup automático | Simulado (sin worker) |
| 25 | Seed / utilidades dev | Implementado (riesgo en producción) |

---

## 6. Roles del Sistema

| Rol | Descripción | Ámbito |
|-----|-------------|--------|
| `superadmin` | Administrador global del SaaS | Cross-tenant |
| `admin` | Administrador de compañía/sucursal | Tenant |
| `manager` | Gerente de sucursal | Branch |
| `supervisor` | Supervisor operacional | Branch |
| `waiter` | Mesero | Branch |
| `cashier` | Cajero | Branch |
| `chef` | Cocinero | Branch |
| `bartender` | Bartender | Branch |
| `accountant` | Contador | Branch |
| `support` | Soporte técnico | Branch |
| `inventarista` | Encargado de inventario | Branch |

---

## 7. Entornos y Despliegue

| Entorno | Configuración | URL |
|---------|--------------|-----|
| Development | `appsettings.Development.json` | localhost:5001 |
| Production | Docker en VPS + nginx SSL | https://restbar.autonomousflow.lat |
| Puerto interno Docker | 8080 → expuesto 8084 | — |

---

## 8. Métricas del Codebase

| Métrica | Cantidad |
|---------|----------|
| Controladores | 26 |
| Servicios (implementaciones) | 36 |
| Interfaces | 34 (+2 en Services/) |
| Entidades EF Core | 29 DbSets |
| Migraciones | 5 |
| Vistas Razor (.cshtml) | 65 |
| Módulos JS custom | 28 |
| Políticas de autorización | 12 |
| Middlewares custom | 3 |
| Background services | 0 |

---

## 9. Flujo Operativo Principal (Resumen)

```
Login → Validación credenciales → Claims (UserId, Role, BranchId, CompanyId)
  → Dashboard (cards por rol)
    → Selección de mesa → Creación/apertura de orden
      → Agregar productos → Verificar stock
        → Enviar a cocina → KDS actualiza vía SignalR
          → Marcar ítems listos → Orden lista para pago
            → Pago parcial/completo/split → Actualizar mesa
              → Orden completada → Reportes/Auditoría
```

---

## 10. Documentos de Este Análisis

| # | Documento | Contenido |
|---|-----------|-----------|
| 01 | APPLICATION_OVERVIEW | Este documento |
| 02 | ARCHITECTURE_ANALYSIS | Patrones, frameworks, diagramas |
| 03 | PROJECT_STRUCTURE | Carpetas, organización |
| 04 | MODULE_CATALOG | Catálogo completo de módulos |
| 05 | COMPLETE_FUNCTIONAL_FLOWS | Flujos de negocio |
| 06 | NAVIGATION_MAP | Mapa de navegación y rutas |
| 07 | BACKEND_ANALYSIS | Controllers, services, middleware |
| 08 | FRONTEND_ANALYSIS | Views, JS, UX |
| 09 | DATABASE_ANALYSIS | Esquema, relaciones |
| 10 | SECURITY_ANALYSIS | Auth, permisos, riesgos |
| 11 | CONFIGURATION_ANALYSIS | appsettings, parámetros |
| 12 | INTEGRATION_ANALYSIS | Integraciones internas/externas |
| 13 | DEPENDENCY_ANALYSIS | Mapa de dependencias |
| 14 | RISK_ASSESSMENT | Riesgos identificados |
| 15 | PRE_PRODUCTION_READINESS | Preparación para certificación |
| 16 | EXECUTIVE_SUMMARY | Resumen ejecutivo |

---

## 11. Observaciones Iniciales

1. **Monolito funcional maduro** en el núcleo POS (órdenes, cocina, pagos) con SignalR en tiempo real.
2. **Sin workers ni jobs** — toda la lógica es request-scoped.
3. **Multi-tenancy a nivel aplicación** — requiere validación rigurosa en certificación E2E.
4. **Módulos periféricos incompletos** — proveedores, compras, impresoras, facturación completa.
5. **Documentación previa existente** en `docs/E2E_AUDITORIA_FUNCIONAL_PREVIA.md` (parcialmente desactualizada respecto a hardening reciente).

---

*Análisis generado sin modificaciones al código. Fase siguiente: Certificación Funcional Enterprise.*
