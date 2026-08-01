# 12 — CASH E2E REPORT (Tab Browser)

**Dominio:** Cash session, register, X/Z, movements  
**Pack fecha:** 2026-08-01  
**Corrida:** chromium-desktop global + E2ETab deep

| ID | Escenario | Estado | Notas |
|----|-----------|--------|-------|
| E2E-CASH-10 | Open cash → POS order → Z/X | **PASS** | Evidence `Evidence/Cash/E2E-CASH-10` |
| E2E-CASH-11 | Foreign session ZReport soft deny | **PASS** | No 500 / no Postgres leak |
| E2E-CASH-12 | Paid-in → paid-out → list movements | **PASS** | API + Detail screenshot |
| CASH suite / CASH-L* | Lifecycle / arqueo | **PASS** | Incluido en global 179 |
| CASH-Z-01 | ZReport empty session UX | **PASS** | ReportMissing |

**Veredicto dominio Cash:** **PASS**
