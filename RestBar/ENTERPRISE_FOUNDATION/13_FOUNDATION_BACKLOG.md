# 13 — FOUNDATION BACKLOG

---

# Implemented in F0.5 (code)

| ID | Ítem | Estado |
|----|------|--------|
| FF-01 | Docs ENTERPRISE_FOUNDATION 01–15 | ✅ |
| FF-02 | TenantScope helper | ✅ (código) |
| FF-03 | FeatureFlags options | ✅ |
| FF-04 | Policies Cash/Purchasing/Costing/Franchise | ✅ |
| FF-05 | Menú Payment → PaymentView | ✅ |
| FF-06 | Fix accountant `/Reports/Index` | ✅ |
| FF-07 | Seed Development-only gate | ✅ |
| FF-08 | Remove password OnConfiguring | ✅ |
| FF-09 | RestBar.Tests smoke project | ✅ |
| FF-10 | Operational indexes migration | ✅ |
| FF-11 | Compile + test gate | ✅ |

---

# Next foundation (F0.6 — aún diseño/ejecutar después)

| ID | Ítem | Pri |
|----|------|-----|
| FF-20 | Extraer OrderService detrás facade | P0 |
| FF-21 | Aplicar TenantScope en Order GetById/mutations | P0 |
| FF-22 | Consolidar dual DbContext DI | P1 |
| FF-23 | AsNoTracking Kitchen listados | P1 |
| FF-24 | Hide SupplierAnalysis via FeatureFlags en UI | P1 |
| FF-25 | WebApplicationFactory integration tests MT | P0 |
| FF-26 | HostedService skeleton (no-op health) | P2 |
| FF-27 | Deprecate ProductCategory plan | P2 |

---

# Test infrastructure backlog

| ID | Suite |
|----|-------|
| FT-01 | Unit: TenantScope |
| FT-02 | Unit: FeatureFlags defaults |
| FT-03 | Integration: auth policies resolve |
| FT-04 | Functional: smoke home/login (manual→auto) |
| FT-05 | Business: order pay cancel (reuse cert scripts) |
| FT-06 | Enterprise: IDOR matrix |
| FT-07 | Performance: KDS P95 baseline |
| FT-08 | Regression: cert PowerShell in CI |
| FT-09 | Smoke: `dotnet test` on PR |
