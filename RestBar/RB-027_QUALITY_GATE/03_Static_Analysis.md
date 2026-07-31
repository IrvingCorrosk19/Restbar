# 03 — Static Analysis

## Herramientas adoptadas / planificadas

| Herramienta | Uso | Estado RB-027 |
|-------------|-----|---------------|
| Compilador C# nullable | Warnings en build | Activo (muchas CS86xx históricas) |
| `dotnet list package --vulnerable` | Supply-chain | **CI G3** + script local |
| Coverlet | Cobertura | **CI G2** artifact |
| Playwright console/CSP checks | Runtime UI | Suites Inventory/Security |
| Roslyn analyzers (NetAnalyzers) | Complejidad / CA rules | **Pendiente** habilitar Enforce |
| Sonar / Semgrep / CodeQL | SAST profundo | **Pendiente** (recomendado P1) |
| NDepend / dependency graph | Ciclos | Manual vía review |

## Hallazgos baseline (evidencia 2026-07-30)

### Seguridad / dependencias

- MailKit 4.14.1 / MimeKit 4.14.0 — advisories **moderados** (aceptados temporalmente; plan upgrade P1).
- **Corregido en RB-027:** eliminados `Microsoft.AspNetCore.SignalR` 1.1.0 y `Npgsql.EntityFrameworkCore.PostgreSQL.Design` 1.1.0 (arrastraban High transitivos). SignalR viene del shared framework .NET 8.
- Gate G3/G8: **falla** si aparecen High/Critical.
- Sin `[Obsolete]` en app source.
- CSP + headers endurecidos (RB-026); DataTables CDN allowlist.

### Complejidad / tamaño (observacional)

| Área | Hallazgo | Riesgo |
|------|----------|--------|
| `OrderService.cs` | Archivo muy grande (&gt;2k líneas) | Alto mantenimiento / regresiones |
| `AdvancedReportsService.cs` | Lógica densa + nullable warnings | Medio |
| `Program.cs` | DI denso, factories manuales | Medio — dificulta test double |
| Controllers fat | Mezcla MVC + JSON API | Medio |

### N+1 / performance

- No hay detector automático en CI.
- Índices enterprise documentados en RB-025/024; smoke PERF soft budget 5s.
- **Riesgo residual:** reportes y listados sin assert de query count.

### Código muerto / duplicado

- Suites browser Smoke vs Regression solapan navegación.
- Certificaciones históricas (`FUNCTIONAL_*`, `ORDER_*`) duplican narrativa — no código runtime.
- `wwwroot/lib` third-party no auditar como deuda propia.

### Memoria

- Sin heap profiling en CI. SignalR + EF scopes: patrón Scoped correcto; riesgo de captive dependency en factories de `Program.cs` → revisar en PRs que toquen DI.

## Reglas de gate estático

1. PR no introduce High/Critical package vulns sin waiver.
2. Archivos nuevos de servicio &gt; 800 LOC → justificación en PR o split.
3. No silenciar nullable con `!` masivo sin revisión.
4. Backlog: CodeQL workflow + `TreatWarningsAsErrors` gradual por carpeta `Domain/`.
