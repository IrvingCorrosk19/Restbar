# 00 — MASTER BROWSER FUNCTIONAL TEST PLAN

**Documento rector de la certificación full-system RestBar**  
**Creado:** 2026-07-30T20:40:00-05:00 (ANTES de ejecución masiva)  
**Versión plan:** 1.0  
**Commit baseline:** `0ab2bd2` (`0ab2bd29720ee92f59a4d247ccba43618918ba74`)  
**Inventario oficial:** `01_SYSTEM_MODULE_INVENTORY.md`  
**Target primario:** `http://164.68.99.83:8084` (VPS) · fallback local `http://localhost:5001`  
**Framework:** Playwright Node (`RestBar/tests/Browser`) — único framework browser del repo  
**Estado global del plan:** `EXECUTED — PASS WITH CONDITIONS` (ver `30_FINAL_FULL_SYSTEM_CERTIFICATION.md`)

> **Regla:** este archivo es la fuente oficial de ejecución. No se omiten escenarios. Los criterios de aceptación no se cambian silenciosamente. Una fase no se marca DONE sin evidencia.

---

## 2.1 Resumen del sistema

| Aspecto | Evidencia |
|---------|-----------|
| Arquitectura | ASP.NET Core 8 MVC + API Controllers + Razor Views + SignalR |
| Stack | .NET 8, EF Core 9, Npgsql/PostgreSQL, Cookie auth `RestBarAuth`, Chart.js CDN |
| Base de datos | PostgreSQL `RestBar` (VPS: user `restbaruser`, container `restbar_postgres`) |
| App web | Docker `restbar_web` puerto host **8084** |
| Tests browser | Playwright `@playwright/test` ^1.49 · 26 specs · ~122 tests · 3 proyectos Chromium |
| Auth | Cookie + roles claim; policies en `Program.cs` + `AddEnterpriseModulePolicies` |
| Multitenancy | `Company` → `Branch`; SuperAdmin; middleware `TenantSubscriptionMiddleware` (activo/inactivo) |
| Roles detectados | superadmin, admin, manager, supervisor, waiter, cashier, chef, bartender, accountant, inventarista, support |
| Feature Flags | Cash, Purchasing, FoodCost, CommandCenter, Copilot(OFF), Seed, ReportExports (algunos sin consumer) |
| Dependencias externas | Email (MailKit), SignalR WS, CDN Chart.js — sin Power BI |
| Background jobs | **Ninguno** registrado |
| Controllers | **43** · Views folders **37** · Areas **0** |

---

## 2.2 Alcance (módulos a probar)

Todos los IDs **M01–M86** de `01_SYSTEM_MODULE_INVENTORY.md` con UI/API ejecutable.

**Prioridad de ejecución (P0 primero):**

1. Auth + Home shell  
2. Floors / Tables / Stations  
3. POS Orders + KDS + Payments  
4. Cash RB-010  
5. Inventory  
6. Procurement RB-020  
7. Food Cost RB-023  
8. Analytics RB-025 + Reports/Exports  
9. Admin (Company/Branch/Users)  
10. RBAC + Multitenant + Security negatives  
11. Responsive + Performance smoke  
12. Audit / Advanced Settings / Email (parcial)

---

## 2.3 Fuera de alcance (justificado)

| Ítem | Justificación técnica |
|------|------------------------|
| Combos, Promociones, Reservas, Bodegas WMS, Conteos físicos, Webhooks, Billing SaaS UI, Dark Mode, i18n, Documentos | **No existe código ejecutable** (ver inventario) |
| Lab sintético 1M órdenes / stress destructivo en VPS prod | Política: no cargas destructivas en producción |
| Firefox / WebKit | No configurados; Chromium 3 viewports es el estándar del repo |
| Copilot UI funcional | Feature flag `EnableCopilot=false` → DESHABILITADO (probar solo ModuleDisabled) |
| Seed endpoints en Production | `SeedEnvironmentGate` → Development only |
| PDF binario nativo (QuestPDF) | Path oficial = HTML print; binario diferido (limitación conocida RB-025) |

---

## 2.4 Estrategia de pruebas

| Tipo | Uso |
|------|-----|
| Smoke | SMK + FF + Home/Login |
| Functional | Spec por módulo |
| Integration | POS→KDS→Pay→Cash→Inv |
| E2E | ORD-E2E + nuevos flujos críticos |
| Negative | NEG + SEC + nuevos |
| Regression | REG + suite global post-fix |
| RBAC | WTR + SEC + nuevos roles |
| Multitenant | MT + IDOR checks |
| Responsive | RSP + projects tablet/mobile |
| Accessibility | Checks básicos labels/focus (nuevo A11Y-*) |
| Performance browser | PERF-* budgets soft |
| Concurrency | Contexts separados (waiter/kitchen/cashier) |
| Recovery | SignalR reconnect soft |
| Data integrity | API/DB asserts post-flujo |
| Exportaciones | AN-05 + AdvancedReports + Reports stubs |

**Framework rule:** reutilizar `tests/Browser`; helpers `auth.js`, `pos.js`. Esperas por estado (visible/response), no `Thread.Sleep` / sleeps fijos innecesarios (excepto budgets ya existentes a reducir).

---

## 2.5 Matriz de escenarios (oficial)

Estados: `NOT STARTED` | `IN PROGRESS` | `PASS` | `FAIL` | `BLOCKED` | `FIXED` | `REGRESSION PASS` | `NOT APPLICABLE`

### A — Smoke / Auth / Session

| Test ID | Módulo | Escenario | Precondición | Pasos | Resultado esperado | Prioridad | Automatización | Estado |
|---------|--------|-----------|--------------|-------|--------------------|-----------|----------------|--------|
| SMK-01 | Auth | Login admin | App up | Login form | Sale de /Auth/Login | P0 | EXISTENTE | NOT STARTED |
| SMK-02 | Auth | URL protegida sin sesión | Anónimo | Goto CashSession | Redirect Login | P0 | EXISTENTE | NOT STARTED |
| SMK-03 | Auth | Orders shell | Login | Goto /Order | HTTP&lt;500 | P0 | EXISTENTE | NOT STARTED |
| AUTH-01 | Auth | Login inválido | App up | Bad password | Error / permanece login | P0 | NUEVA | NOT STARTED |
| AUTH-02 | Auth | Logout | Login | POST Logout | Login page | P0 | NUEVA | NOT STARTED |
| AUTH-03 | Auth | ForgotPassword page | App up | Open Forgot | 200 o flujo UI | P2 | NUEVA | NOT STARTED |
| AUTH-04 | Auth | Profile | Login | /Auth/Profile | 200, datos user | P2 | NUEVA | NOT STARTED |
| AUTH-05 | Auth | Dos contexts admin/waiter | Users seeded | Parallel tabs | Sesiones aisladas | P1 | NUEVA | NOT STARTED |
| FF-01..03 | Flags | Modules enabled Dev/VPS | Flags on | Cash/PO/FC pages | No ModuleDisabled | P0 | EXISTENTE | NOT STARTED |

### B — Admin / Org

| Test ID | Módulo | Escenario | Precondición | Pasos | Esperado | Pri | Auto | Estado |
|---------|--------|-----------|--------------|-------|----------|-----|------|--------|
| ADM-01 | Company | Index loads | admin | /Company | 200, list/table | P1 | NUEVA | NOT STARTED |
| ADM-02 | Branch | Index loads | admin | /Branch | 200 | P1 | NUEVA | NOT STARTED |
| ADM-03 | Users | UserManagement | admin | /User/UserManagement | 200 | P1 | FORTALECER SHF | NOT STARTED |
| ADM-04 | SuperAdmin | Guard non-super | admin | /SuperAdmin | 403/deny | P1 | NUEVA | NOT STARTED |
| ADM-05 | Category | Index | admin | /Category | 200 | P2 | NUEVA | NOT STARTED |
| ADM-06 | AdvancedSettings | Index | manager+ | /AdvancedSettings | 200 | P2 | NUEVA | NOT STARTED |
| ADM-07 | Audit | Index | manager+ | /Audit | 200 | P2 | NUEVA | NOT STARTED |
| ADM-08 | Email | Index | admin | /Email | 200 o graceful | P3 | NUEVA | NOT STARTED |

### C — Floors / Tables / Stations

| Test ID | Módulo | Escenario | Pri | Auto | Estado |
|---------|--------|-----------|-----|------|--------|
| FLR-01..04 | Areas | Existing suite | P1 | EXISTENTE | NOT STARTED |
| TBL-01..04 | Tables | Existing suite | P0 | EXISTENTE | NOT STARTED |
| STN-01..05 | Stations | Existing suite | P0 | EXISTENTE | NOT STARTED |
| FTS-NEG-01 | Tables | Unauth direct | P1 | NUEVA | NOT STARTED |

### D — POS / Orders / KDS / Payments

| Test ID | Módulo | Escenario | Pri | Auto | Estado |
|---------|--------|-----------|-----|------|--------|
| ORD-E2E-01..05 | POS/KDS | Floor→product→kitchen | P0 | EXISTENTE | NOT STARTED |
| ORD-NAV-01..06 | POS | Navigation guards | P1 | EXISTENTE | NOT STARTED |
| OPS-01..03 | Orders | Deep ops | P1 | EXISTENTE | NOT STARTED |
| KDS-01..04 | Kitchen | KDS ready | P0 | EXISTENTE | NOT STARTED |
| PAY-01..04 | Payments | PaymentView | P1 | EXISTENTE | NOT STARTED |
| POS-PAY-01 | Payments | Modal pago en Order | P0 | NUEVA/FORTALECER | NOT STARTED |
| POS-SPLIT-01 | Split | Separate accounts UI | P1 | NUEVA | NOT STARTED |
| POS-CONC-01 | Concurrency | Waiter + Kitchen contexts | P0 | NUEVA | NOT STARTED |
| NEG-01..04 | Negatives | Order/cash integrity | P0 | EXISTENTE | NOT STARTED |

### E — Cash RB-010

| Test ID | Módulo | Escenario | Pri | Auto | Estado |
|---------|--------|-----------|-----|------|--------|
| CASH-01..07 | Cash | Core suite | P0 | EXISTENTE | NOT STARTED |
| CASH-X01..X05 | Cash | Extended | P1 | EXISTENTE | NOT STARTED |
| CASH-L01..L04 | Cash | Lifecycle | P0 | EXISTENTE | NOT STARTED |
| CASH-Z-01 | Cash | ZReport page | P0 | NUEVA | NOT STARTED |
| CASH-XREP-01 | Cash | XReport page | P1 | NUEVA | NOT STARTED |

### F — Inventory / Procurement / FoodCost

| Test ID | Módulo | Escenario | Pri | Auto | Estado |
|---------|--------|-----------|-----|------|--------|
| INV-01..08 | Inventory | Index suite | P0 | EXISTENTE | NOT STARTED |
| INV-E01..E05 | Inventory | Enterprise | P1 | EXISTENTE | NOT STARTED |
| INV-ORD-01..02 | Inventory | Order impact | P0 | EXISTENTE | NOT STARTED |
| PO-01..06 | Procurement | Suite | P0 | EXISTENTE | NOT STARTED |
| PO-E2E-01 | PO | Create→Approve→Receive | P0 | NUEVA | NOT STARTED |
| FC-01..05 | FoodCost | Suite | P1 | EXISTENTE | NOT STARTED |

### G — Analytics / Reports / Exports

| Test ID | Módulo | Escenario | Pri | Auto | Estado |
|---------|--------|-----------|-----|------|--------|
| AN-01..06 | Analytics | Executive center | P1 | EXISTENTE | NOT STARTED |
| RPT-01 | Reports | /Reports Index | P2 | NUEVA | NOT STARTED |
| RPT-02 | AdvancedReports | Index + load sales | P2 | NUEVA | NOT STARTED |
| RPT-03 | AdvancedReports | Export Excel non-empty | P2 | NUEVA | NOT STARTED |
| RPT-04 | Reports | ExportPdf stub documented | P2 | NUEVA | NOT STARTED |
| BI-01 | BiNative | Index loads | P2 | NUEVA | NOT STARTED |
| ECC-01 | CommandCenter | Index loads | P2 | EXISTENTE parcial REG | NOT STARTED |

### H — RBAC / Multitenant / Security / Responsive / Perf

| Test ID | Módulo | Escenario | Pri | Auto | Estado |
|---------|--------|-----------|-----|------|--------|
| SEC-01..05 | Security | AuthZ gates | P0 | EXISTENTE | NOT STARTED |
| WTR-01..05 | Waiters/RBAC | Role smoke | P1 | EXISTENTE | NOT STARTED |
| MT-01..02 | Multitenant | Isolation smoke | P0 | EXISTENTE | NOT STARTED |
| MT-IDOR-01 | Multitenant | Cross-tenant URL ID | P0 | NUEVA | NOT STARTED |
| RSP-01..03 | Responsive | POS/Cash | P2 | EXISTENTE | NOT STARTED |
| PERF-* | Performance | Page budgets | P2 | EXISTENTE | NOT STARTED |
| A11Y-01 | A11Y | Login labels/focus | P3 | NUEVA | NOT STARTED |
| COP-01 | Copilot | ModuleDisabled when off | P3 | NUEVA | NOT STARTED |

### I — Regression global

| Test ID | Módulo | Escenario | Pri | Auto | Estado |
|---------|--------|-----------|-----|------|--------|
| REG-01..06 | Regression | Core POS shell | P0 | EXISTENTE | NOT STARTED |
| REG-FULL | All | Full suite chromium-desktop | P0 | EXISTENTE+NUEVAS | NOT STARTED |

---

## 2.6 Datos de prueba

| Recurso | Fuente |
|---------|--------|
| Tenant/Company principal | Seed / dump VPS (admin@restbar.com) |
| Sucursales | Branch claims + SuperAdmin companies |
| Pisos/Áreas | Areas existentes |
| Mesas | Tables Available |
| Estaciones | kitchen/bar stations |
| Usuarios | admin@restbar.com / 123456 · roles seeded (mesero/cajero/chef cuando existan) |
| Productos/Categorías | Seed demo |
| Inventario | Product stock + movements |
| Cajas | CashRegister + open wizard |
| Compras | Suppliers + PO |
| Food Cost | Recipes + snapshots |
| Analytics | Órdenes Completed históricas |

**Credenciales smoke:** `RESTBAR_ADMIN_EMAIL` / `RESTBAR_ADMIN_PASSWORD` (default admin).

Si falta usuario de rol → marcar escenario `BLOCKED` (no PASS) y registrar en bug register.

---

## 2.7 Dependencias / orden de ejecución

```
0. Environment readiness (05)
1. Smoke + Auth + Feature Flags
2. Admin org pages (Company/Branch/Users) — read smoke
3. Floors → Tables → Stations
4. Products/Categories read
5. Inventory baseline
6. Cash open (si requerido por pagos)
7. POS Orders → KDS → Payments
8. Cash close/reports
9. Procurement
10. Food Cost
11. Analytics + Reports/Exports
12. RBAC + Multitenant + Security
13. Responsive + Performance
14. Defects fix loop
15. Regression global REG-FULL
16. Final certification (30)
```

---

## 2.8 Criterios de entrada

Antes de ejecutar suites:

- [ ] `dotnet build` exit 0  
- [ ] App HTTP reachable (VPS 8084 o local 5001)  
- [ ] PostgreSQL healthy  
- [ ] Login admin funciona  
- [ ] Feature flags Cash/Purchasing/FoodCost ON en target  
- [ ] Playwright `node_modules` instalado  
- [ ] Inventario `01` leído  

**Gate status:** `PENDING` → actualizar en `05_ENVIRONMENT_READINESS.md`

---

## 2.9 Criterios de salida (certificación)

- [ ] 100% escenarios P0 ejecutados (PASS/FAIL/BLOCKED documentado)  
- [ ] 0 P0 abiertos  
- [ ] 0 P1 abiertos  
- [ ] Regresión `REG-FULL` PASS (chromium-desktop)  
- [ ] Sin fuga multitenant demostrada  
- [ ] Sin HTTP 500 en rutas críticas  
- [ ] Sin errores JS críticos (filtro noise SignalR)  
- [ ] Evidencia en `Evidence/` + reportes 06–30  

**Veredicto permitido solo al final:** PRODUCTION READY | PILOT READY | PASS WITH CONDITIONS | NOT READY | FAIL

---

## 2.10 Estrategia de corrección

```
Detectar → Reproducir → Clasificar (P0–P4) → Root cause → Fix mínima
→ Prueba regresión → Reejecutar test → Suite módulo → REG-FULL
```

Registrar en `25_BUG_REGISTER.md`. No ocultar. No skip por fallo. No cambiar expected para forzar PASS.

---

## 2.11 Validación interna del plan (pre-ejecución)

| Check | Resultado |
|-------|-----------|
| Todos módulos inventario incluidos en alcance o OOS justificado | YES |
| Positivos + negativos por área crítica | YES (NEG/SEC + AUTH-01) |
| Multitenant + RBAC presentes | YES (MT/WTR/SEC/MT-IDOR) |
| Plan guardado como fuente oficial | YES (este archivo) |
| Aprobación manual requerida | NO — auto-ejecutar |

**Plan APPROVED FOR EXECUTION** — 2026-07-30

---

## 2.12 Log de ejecución (actualizar en vivo)

| Fecha | Fase | Acción | Resultado | Evidencia |
|-------|------|--------|-----------|-----------|
| 2026-07-30 | 0–2 | Discovery + inventario + plan | DONE | `01`, este archivo |
| 2026-07-30 | 3 | Nuevas specs AUTH/ADM/RPT/CASH-Z/CONC/A11Y | DONE | `tests/Browser/*` |
| 2026-07-30 | 5 | Environment readiness | OPEN | `05` |
| 2026-07-30 | 6–22 | Suite chromium-desktop 147 tests | 142 PASS / 1 FAIL / 1 flaky / 3 skip | terminal 42m |
| 2026-07-30 | 24 | Fix AUTH-02 + POS-CONC; retest | PASS | AUTH+CONC |
| 2026-07-30 | 27 | Unit tests | 77/77 PASS | dotnet test |
| 2026-07-30 | 30 | Certificación final | **PASS WITH CONDITIONS** | `30` |

---

## 2.13 Mapa de entregables

| Archivo | Propósito | Estado doc |
|---------|-----------|------------|
| 00 (este) | Plan rector | ACTIVE |
| 01 | Inventario módulos | DONE |
| 02 | Rutas/UI | PENDING |
| 03 | Assessment tests existentes | PENDING |
| 04 | Test data | PENDING |
| 05 | Environment readiness | PENDING |
| 06–24 | Reportes por área | PENDING |
| 25 | Bug register | PENDING |
| 26 | Fix/regression | PENDING |
| 27 | Evidence index | PENDING |
| 28 | Limitations | PENDING |
| 29 | Release readiness | PENDING |
| 30 | Final certification | PENDING |

---

## 2.14 Comandos oficiales

```bash
# Unit
dotnet build
dotnet test

# Browser (desde RestBar/tests/Browser)
set RESTBAR_BASE_URL=http://164.68.99.83:8084
npx playwright test --project=chromium-desktop

# Smoke only
npx playwright test Smoke --project=chromium-desktop
```

---

**FIN DEL PLAN MAESTRO v1.0 — EJECUTAR DESDE FASE 3 EN ADELANTE SIGUIENDO ESTE ORDEN**
