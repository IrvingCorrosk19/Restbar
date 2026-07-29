# 01 — ARCHITECTURE AUDIT

**Programa:** RestBar Enterprise Foundation (Fase 0.5)  
**Fecha:** 2026-07-29  
**Stack:** ASP.NET Core 8 MVC · EF Core 9 · PostgreSQL · SignalR · Cookie Auth

---

# Veredicto arquitectónico

RestBar es un **monolito MVC operativo maduro en POS/KDS**, con **límites de dominio débiles**, **God objects** en Order, **multitenancy manual inconsistente**, **cero tests automatizados** y **cero background jobs**.

La base **puede soportar 5 años** si se paga deuda en este orden: tenant helpers → índices → policies enterprise → test harness → extracción gradual de OrderService → jobs → módulos Cash/PO encima de building blocks.

---

# 1. Topología actual

```
Controllers (fat) → Services (fat) → RestBarContext → PostgreSQL
                         ↓
                   OrderHub (SignalR)
Middleware: Auth → Audit → Error → Permission → TenantSubscription
```

**Sin:** Repositories formales · CQRS · MediatR · Outbox · HostedServices · Global query filters tenant.

---

# 2. Dónde crecerá más el sistema (5 años)

| Zona | Crecimiento | Riesgo si no se prepara |
|------|-------------|-------------------------|
| Inventory + Purchasing + Costing | Muy alto | OrderService + InventoryOps se vuelven inmantenibles |
| Cash + Fiscal (Invoice) | Alto | Lógica en PaymentController |
| Reporting → BI → Copilot | Alto | AdvancedReportsService God object + OLTP abuse |
| Multitenant / Franchise / Billing | Alto | Filtros manuales → IDOR |
| Order/POS | Medio (estabilizar) | Ya demasiado grande |

---

# 3. God objects / responsabilidades

| Artefacto | ~KB / LOC | Problema |
|-----------|-----------|----------|
| `OrderService.cs` | 129 KB / ~2300 LOC | Lifecycle + KDS + pricing + table + inventory hooks |
| `OrderController.cs` | 90 KB / ~1900 LOC | UI + API + kitchen mezclados |
| `RestBarContext.cs` | 78 KB | Todo el schema fluent en un archivo |
| `AdvancedReportsService.cs` | 59 KB / ~1200 LOC | Todos los reportes + stubs |
| `SeedController.cs` | 52 KB | Seed + AllowAnonymous |

**Patrón requerido:** Facade `IOrderService` estable + extracción interna (`OrderLifecycle`, `KitchenQuery`, `OrderPricing`) **sin cambiar contratos HTTP**.

---

# 4. DI / Program.cs

- Dual registration `AddDbContext` + `AddScoped<RestBarContext>` factory → **olor**; consolidar con ctor `IHttpContextAccessor`.
- Muchos factories manuales (Station, Area, User, Payment…) → inconsistente vs `AddScoped<T,TImpl>`.
- **Ningún** `AddHostedService`.
- Policies actuales cubren ops; **faltan** CashAccess, PurchasingAccess, CostingAccess, FranchiseAccess (preparar).

---

# 5. Middleware

Orden correcto en general. Gaps:

- `/Seed` whitelisted en Permission + TenantSubscription  
- Seed AllowAnonymous si env ≠ Production (Staging expuesto)  

---

# 6. Patrones a aplicar (sin reescritura total)

| Patrón | Aplicación |
|--------|------------|
| Module folders (`Domain/*`, `Infrastructure/*`) | Organización; Controllers quedan en root MVC |
| Tenant scope helper | Todas las mutaciones GetById |
| Feature flags | Ocultar stubs / gates de módulos futuros |
| Hosted services | Backup, BI aggregate, alerts |
| Adapter | Fiscal por país |
| Ledger pattern | InventoryMovement + CashMovement futuros |
| Snapshot aggregate API | Command Center (<5s) |

---

# 7. Deuda que debe pagarse PRIMERO (foundation)

1. Helper tenant + tests IDOR  
2. Índices compuestos operativos  
3. Policies + feature flags enterprise  
4. Proyecto de tests + smoke  
5. Harden Seed / secrets en OnConfiguring  
6. Unificar menús rotos (Payment URL)  
7. Plan de extracción OrderService (no ejecutar completo en 0.5)

---

# 8. Lo que NO hacer en foundation

- No crear tablas Cash/PO/Combo aún  
- No reescribir SPA  
- No microservicios  
- No partir DbContext en múltiples contexts  
- No cambiar comportamiento POS/KDS certificado
