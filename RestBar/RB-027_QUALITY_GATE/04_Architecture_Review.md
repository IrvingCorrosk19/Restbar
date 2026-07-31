# 04 — Architecture Review

**Baseline alineado con** `RB-026_PRODUCTION_READY/01_Architecture.md`

## Capas detectadas

```
Views (Razor) + wwwroot
        ↓
Controllers (MVC + JSON actions) + Hubs (SignalR)
        ↓
Services / Domain helpers (Cash SM, Procurement SM, Analytics)
        ↓
EF Core RestBarContext + PostgreSQL
```

| Principio | ¿Respetado? | Notas |
|-----------|-------------|-------|
| Modular monolith | Sí | Feature flags por módulo enterprise |
| Controllers delgados | Condicional | Varios controllers con lógica |
| Servicios por interfaz | Sí (mayoría) | DI en `Program.cs` + Extensions |
| Repositorios explícitos | Parcial | Muchos servicios usan `DbContext` directo |
| DTOs vs Entities | Condicional | ViewModels + entidades expuestas en JSON a veces |
| Multitenant en borde | Sí | Middleware + claims Company/Branch |
| No circular deps de proyectos | Sí | Un web project + Tests |

## DI / IoC

- Registro central en `Program.cs` (+ `EnterpriseCashExtensions`, etc.).
- Algunos servicios construidos con factories `provider => new ...` — dificulta unit tests y viola pureza DI.
- **Regla RB-027:** nuevos servicios → `AddScoped&lt;IFoo, Foo&gt;` sin `new` en factory salvo necesidad documentada.

## APIs

- No hay proyecto API separado; endpoints JSON conviven con MVC.
- Health: `/health`, `/health/live`, `/health/ready` (RB-026).
- **Regla:** contratos JSON breaking → versionado o changelog + tests browser/API.

## Persistencia

- EF Core 9 + Npgsql; migraciones en `Migrations/` (17 no-designer).
- Retry on failure habilitado (RB-026).
- **Regla:** toda migración en PR debe incluir notas de rollback + impacto downtime.

## Deuda arquitectónica aceptada (no ampliar)

1. Monolito modular (no microservicios).
2. Sin message bus / outbox.
3. Sin capa Repository formal universal.
4. Cobertura unitaria baja en servicios grandes.

## Veredicto arquitectura

**Mantenible con condiciones** para pilot/enterprise modular. No introducir capas nuevas “por moda”. Evolucionar tests + DI limpio + reducir god-services.
