# 00 — MASTER E2E TAB BROWSER TEST PLAN

**Programa:** RestBar FULL E2E Tab Browser Certification  
**Fecha plan:** 2026-08-01  
**Cierre ejecución:** 2026-08-01 — **PASS WITH CONDITIONS** (`29_FINAL_FUNCTIONAL_CERTIFICATION.md`)  
**Ambiente objetivo:** VPS `http://164.68.99.83:8084` (+ local `RESTBAR_BASE_URL`)  
**Fuente de verdad:** este documento + `29_*` (estados cerrados en ejecución global 179 PASS / 1 skip)

---

## 1. Objetivo

Demostrar con evidencia de navegador real (Playwright + contextos independientes) que RestBar opera E2E:

tenant → empresa → sucursal → pisos/mesas/estaciones → pedido → KDS → pago → caja → inventario → compras → food cost → BI/reportes  

sin contaminación entre Tenant A/B/C.

**Regla:** no PASS sin ejecución real. No inventar resultados. BLOCKED ≠ PASS.

---

## 2. Inventario de módulos (resumen)

| Dominio | Módulos | UI | Flag / Policy | Estado plan |
|---------|---------|----|---------------|-------------|
| Auth | Login, MFA, Logout, Forgot/Reset, Profile | Auth/* | — | READY |
| Admin | Company, Branch, User, UserMgmt, Assignments, SuperAdmin | Views | Franchise / UserManagement | READY |
| Config | Area, Table, Station, Category, Product, Modifier, Customer, AdvancedSettings, Email | Views | ProductAccess / SystemConfig | READY |
| POS | Order Index, SendToKitchen, MoveToTable | Order + _OrderLayout | OrderAccess | READY |
| KDS | StationOrders kitchen/bar, KitchenApi | Order/Kitchen | KitchenAccess | READY |
| Payments | Payment API, PaymentView, Person split | API + Views | PaymentAccess | READY |
| Cash | Session, Register, Movement API, X/Z | Cash* | EnableCashModule + CashAccess | READY |
| Inventory | Index, Movement API, StockTransfer API, Assignments | Partial UI | InventoryAccess (sin flag) | READY |
| Procurement | Supplier, PO, Dashboard | Views | EnablePurchasingModule | READY |
| FoodCost | Dashboard, Recipe, Menu Eng | Views | EnableFoodCostModule | READY |
| Intelligence | DI, BusinessRules, Copilot | Views | DI/BR on; Copilot **off** Prod | READY / N/A Copilot |
| Analytics | ExecutiveAnalytics, BiNative, CommandCenter | Views | EnableCommandCenter | READY |
| Reports | Reports, AdvancedReports | Views (no nav principal) | ReportAccess | READY |
| Platform | SignalR, Health, Audit, Shift | Mixed | — | READY |

Detalle: `01_COMPLETE_MODULE_INVENTORY.md`.

---

## 3. Orden de ejecución oficial

1. Environment readiness + seed 3 companies (si falta)  
2. Auth + MFA smoke (multi-context)  
3. Admin / floors / tables / stations  
4. Multitenant isolation hostile  
5. POS + KDS multitab (mesero | cocina | bar | cajero)  
6. Payments / split / cancel  
7. Cash open → ops → X/Z  
8. Inventory + recipe consumption  
9. Procurement  
10. Food Cost  
11. BI / reports / forecast  
12. RBAC / SoD  
13. SignalR multitab  
14. Responsive / a11y  
15. Global regression (chromium-desktop full suite)  
16. Final certification doc `29_*`

---

## 4. Matriz de escenarios (IDs maestros)

### AUTH
| ID | Escenario | Estado |
|----|-----------|--------|
| E2E-AUTH-01 | Login admin + MFA challenge | NOT STARTED |
| E2E-AUTH-02 | Logout limpia acceso | NOT STARTED |
| E2E-AUTH-03 | Contextos A/B cookies aisladas | NOT STARTED |
| E2E-AUTH-04 | ForgotPassword reachable | NOT STARTED |

### MT
| ID | Escenario | Estado |
|----|-----------|--------|
| E2E-MT-01 | Costa vs Norte product exclusivity | NOT STARTED |
| E2E-MT-02 | IDOR order GUID ajeno → 403/404 | NOT STARTED |
| E2E-MT-03 | Report filters no filtran otro CompanyId | NOT STARTED |
| E2E-MT-04 | Cash session ajena → deny | NOT STARTED |
| E2E-MT-05 | Tres tenants login concurrente | NOT STARTED |

### POS / KDS / PAY
| ID | Escenario | Estado |
|----|-----------|--------|
| E2E-POS-01 | Mesa → productos → SendToKitchen | NOT STARTED |
| E2E-POS-02 | Multitab: mesero + kitchen + bar | NOT STARTED |
| E2E-POS-03 | MoveToTable | NOT STARTED |
| E2E-PAY-01 | Payment summary / partial API shape | NOT STARTED |
| E2E-PAY-02 | GetPaymentHistory tenant guard | NOT STARTED |

### CASH
| ID | Escenario | Estado |
|----|-----------|--------|
| E2E-CASH-01 | Dashboard enabled | NOT STARTED |
| E2E-CASH-02 | Open wizard | NOT STARTED |
| E2E-CASH-03 | Z/X report pages no 500 | NOT STARTED |

### INV / PO / FC / BI
| ID | Escenario | Estado |
|----|-----------|--------|
| E2E-INV-01 | Inventory index + no stub | NOT STARTED |
| E2E-PO-01 | Supplier + PO list | NOT STARTED |
| E2E-FC-01 | FoodCost + recipes | NOT STARTED |
| E2E-BI-01 | Executive + DI soft | NOT STARTED |
| E2E-RPT-01 | Reports + Advanced export | NOT STARTED |

### PLATFORM
| ID | Escenario | Estado |
|----|-----------|--------|
| E2E-RBAC-01 | Role smoke waiters/cashier/chef | NOT STARTED |
| E2E-SIG-01 | Kitchen update no cross-tenant (soft) | NOT STARTED |
| E2E-UX-01 | Responsive POS/Cash | NOT STARTED |
| E2E-REG-01 | Full chromium-desktop regression | NOT STARTED |

Estados permitidos: NOT STARTED | READY | IN PROGRESS | PASS | FAIL | BLOCKED | FIXED | RETEST PASS | REGRESSION PASS | NOT APPLICABLE | NOT IMPLEMENTED

---

## 5. Datos de prueba controlados

| Tenant | Empresa | Sucursal seed | Admin | Prefijo mesa |
|--------|---------|---------------|-------|--------------|
| A | Restaurante Costa | Costa Centro | admin@costa.restbar.com | C |
| B | Restaurante Norte | Norte Mall | admin@norte.restbar.com | NM |
| C | Restaurante Sur | Sur Hotel | admin@sur.restbar.com | S |
| Demo | RestBar default | (branch claim) | admin@restbar.com | — |

Password: `123456`. MFA TOTP seed cert: `JBSWY3DPEHPK3PXP` (`RESTBAR_MFA_SECRET`).  
Pisos/estaciones: ver `ThreeCompaniesCertSeeder` (Piso 1/2 Salón Terraza Costa; Norte Mall; Sur Hotel/Rooftop).

Detalle: `04_TEST_DATA_MATRIX.md`.

---

## 6. Infraestructura Tab Browser

- Playwright `tests/Browser`  
- Proyectos: chromium-desktop (cert primario), tablet, mobile  
- Helpers: `auth.js` (MFA), `pos.js`, **`multi-context.js`** (contextos aislados)  
- Evidencia: `FULL_E2E_TAB_BROWSER_CERTIFICATION/Evidence/{Module}/{TestId}/`  
- Reporter JSON/HTML existente + logs E2E

---

## 7. Criterios PASS globales

Ver mandato usuario § CRITERIOS OBLIGATORIOS. Resumen:

- Cero P0/P1 abiertos  
- Aislamiento MT en browser + API  
- Pedidos/caja/inventario/reportes sin cruce  
- Regresión chromium-desktop sin FAIL  
- Evidencia screenshots/traces por caso crítico  

---

## 8. Veredictos a emitir (`29_FINAL_FUNCTIONAL_CERTIFICATION.md`)

- FULL E2E FUNCTIONAL CERTIFICATION: PASS | PASS WITH CONDITIONS | FAIL  
- MULTITENANT / RBAC&SOD / DATA / FINANCIAL / INVENTORY / REPORT / CONFIG / SIGNALR / BROWSER / REGRESSION: PASS | FAIL  

---

## 9. Dependencias / bloqueos conocidos (pre-ejecución)

| Ítem | Impacto | Mitigación |
|------|---------|------------|
| `EnableSeedEndpoints=false` en Production | No seed vía HTTP en VPS | Seed previo / SQL / Dev gate |
| Copilot flag off | Copilot ModuleDisabled | NOT APPLICABLE o soft |
| StockTransfer / CashMovement sin UI completa | E2E UI parcial | API + soft UI |
| Nav sin Inventory/Reports | Acceso por URL directa | Casos usan goto path |
| MFA obligatorio privilegios | Todos logins admin/manager | `completeMfaIfNeeded` |

---

## 10. Registro de progreso

| Fecha | Acción | Resultado |
|-------|--------|-----------|
| 2026-08-01 | Plan maestro + discovery 01–05 | DONE |
| 2026-08-01 | E2ETab multi-context specs + evidencia | DONE |
| 2026-08-01 | BUG-E2E-001 rate limit auth 5→60 | FIXED / RETEST PASS |
| 2026-08-01 | E2ETab retest | **5/5 PASS** |
| 2026-08-01 | Global chromium-desktop regression | IN PROGRESS |

### Matriz §4 actualizada (casos Tab nuevos)

| ID | Estado |
|----|--------|
| E2E-AUTH-03 | PASS |
| E2E-MT-02 | PASS |
| E2E-MT-05 | PASS (retest) |
| E2E-POS-01 | PASS |
| E2E-POS-02 | PASS |
| E2E-REG-01 | IN PROGRESS |