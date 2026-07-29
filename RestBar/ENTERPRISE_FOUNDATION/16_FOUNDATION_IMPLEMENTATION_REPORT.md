# 16 — FOUNDATION IMPLEMENTATION REPORT

**Fecha:** 2026-07-29  
**Fase:** 0.5 Enterprise Foundation  
**Resultado:** ✅ Build OK · ✅ Tests 10/10 Passed · ✅ Migration indexes applied

---

# Qué se implementó (seguro, sin cambiar comportamiento POS/KDS)

| Cambio | Archivos | Impacto funcional |
|--------|----------|-------------------|
| `TenantScope` + `ITenantScopeAccessor` | `Infrastructure/Foundation/TenantScope.cs` | Ninguno hasta opt-in |
| `FeatureFlags` | `FeatureFlags.cs` + `appsettings.json` | Config lista; UI aún no filtra |
| Policies Cash/Purchasing/Costing/Franchise | `EnterpriseFoundationExtensions.cs` + `Program.cs` | Nuevas policies; no cambian las existentes |
| Seed solo Development | `SeedEnvironmentGate` + `SeedController` | Staging/Production ya no pueden seed anónimo |
| Secrets fuera de OnConfiguring | `RestBarContext.cs` | Fail-fast si no hay DI |
| Menú Pagos → PaymentView | `AuthorizationHelper.cs` | Corrige link roto (API vs UI) |
| Typo accountant Reports | `AuthorizationHelper.cs` | Corrige `/Report` → `/Reports` |
| Índices operativos | Migration + `RestBarContext` | Solo performance; CREATE IF NOT EXISTS |
| Proyecto tests | `RestBar.Tests` | 10 unit tests foundation |

---

# Qué NO se implementó (correcto para 0.5)

- Caja, Compras, Food Cost, BI, Copilot, Combos, tablas nuevas de negocio  
- Extracción OrderService (plan F0.6)  
- Aplicar TenantScope a OrderService mutaciones (F0.6)  
- Consolidar dual DbContext  
- Ocultar SupplierAnalysis en Views (flag listo; wiring UI pendiente FF-24)

---

# Verificación

```
dotnet build RestBar → succeeded
dotnet test RestBar.Tests → Passed: 10
dotnet ef database update → EnterpriseFoundationOperationalIndexes applied
```

---

# Backlog actualizado

Ver `13_FOUNDATION_BACKLOG.md` — FF-01…FF-11 ✅.

**Siguiente:** F0.6 Order extraction + TenantScope on mutations → luego F1 Caja.

---

# Riesgos residuales

| Riesgo | Mitigación |
|--------|------------|
| Designer migration aún lista drops | Up/Down reescritos a SQL aditivo; OK |
| MSB3277 version conflict en tests | Warning; tests pasan; alinear EF packages después |
| FeatureFlags no cableados a Views | Hacer en FF-24 antes de demo comercial |
