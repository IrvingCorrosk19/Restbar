# 02 — Test Coverage Matrix

**Inventario fecha:** 2026-07-30  
**Evidencia unit:** 77 PASS · line coverage **0.41%** (421/100452)  
**Browser:** 32 specs · ~158 `test()` · projects desktop/tablet/mobile

## Clasificación del inventario

| Tipo | Activos | Calidad | Duplicidad | Mantenimiento | Tiempo típico | Confiabilidad |
|------|---------|---------|------------|---------------|---------------|---------------|
| Unitarias | 8 archivos · 19 clases · 77 casos | Alta en math/SM; baja en servicios | Baja | Baja (puros) | ~0.1–1 s | Alta |
| Integración / API | **0** harness | N/A | — | — | — | **Hueco crítico** |
| Browser E2E | 32 specs | Media–alta funcional | Media (smoke vs deep) | Media (UI selectors) | ~3–8 min desktop Inventory alone; full suite ~15–40+ min | Media (env/login) |
| DB | Ningún test de migración/constraints | — | — | — | — | Hueco |
| Seguridad | Browser Security + a11y/IDOR | Media | Baja | Media | ~1–3 min | Media |
| Performance | 5 páginas budget soft | Baja profundidad | Baja | Baja | ~1–2 min | Media (red VPS) |
| Multitenant | Unit TenantScope + 2 browser | Baja profundidad cross-tenant | Baja | Baja | &lt;1 min | Media |
| Responsive | 3 tests + 3 projects | Media | Baja | Baja | ×3 viewports | Media |
| Exportaciones | Analytics flatten unit + UI export | Baja | Baja | Baja | — | Media |

## Matriz módulo × capa

| Módulo | Unit | API | Browser | Integration | Perf | Security | Estado |
|--------|------|-----|---------|-------------|------|----------|--------|
| POS / Orders | ❌ | ❌ | ✅ (~15) | ❌ | ✅ path | Parcial | **PROTEGIDO PARCIAL** |
| Cash | ✅ SM/hash | ❌ | ✅ (~20) | ❌ | ✅ | Parcial | **PROTEGIDO** |
| Inventory | Parcial math | ❌ | ✅ (15) | ❌ | — | Parcial | **PROTEGIDO PARCIAL** |
| Purchases / PO | ✅ SM | ❌ | ✅ (6) | ❌ | ✅ | Parcial | **PROTEGIDO** |
| Food Cost | ✅ math | ❌ | ✅ (5) | ❌ | ✅ | — | **PROTEGIDO** |
| BI / Analytics | ✅ | ❌ | ✅ (6) | ❌ | ✅ | — | **PROTEGIDO** |
| Reports | ❌ | ❌ | ✅ (6) | ❌ | — | — | **PROTEGIDO DÉBIL** |
| Exports | Parcial flatten | ❌ | Parcial | ❌ | — | — | **PROTEGIDO DÉBIL** |
| Auth / RBAC | Parcial (flags/policy) | ❌ | ✅ Auth+Security | ❌ | — | ✅ | **PROTEGIDO PARCIAL** |
| Multitenancy | ✅ TenantScope | ❌ | ✅ (2 shallow) | ❌ | — | Parcial | **PROTEGIDO PARCIAL** |
| Payments | ❌ | ❌ | ✅ (4) | ❌ | — | Parcial | **PROTEGIDO DÉBIL** |
| StockTransfer | ❌ | ❌ | Soft reject | ❌ | — | — | **HUECO** |
| Recipe / Supplier | Parcial / ❌ | ❌ | vía FC/PO | ❌ | — | Soft | **PROTEGIDO DÉBIL** |
| Kitchen / Tables / Stations | ❌ | ❌ | ✅ | ❌ | — | — | **PROTEGIDO PARCIAL** |

## Ningún crítico sin pruebas — estado real

- **Con browser y/o unit:** Cash, PO, FoodCost, BI, Inventory (browser), Orders (browser), Reports (browser), Auth (browser).
- **Sin protección adecuada:** StockTransfer, Payment (sin unit/API), Reports (sin unit), suite **API/Integration** inexistente, cobertura de código global &lt;1%.

## Objetivos 90 días

| Objetivo | Meta |
|----------|------|
| Integration harness | `WebApplicationFactory` + Postgres testcontainer o Testcontainers |
| Unit Orders/Payment | State invariants + validation paths |
| Coverage floor | ≥ 15% líneas en `Domain`/`Services` críticos (Cash, Order, Inventory, Payment) |
| MT | Test IDOR cross-company en API JSON |
| CI | `RESTBAR_BASE_URL` variable + G4 siempre en PRs |
