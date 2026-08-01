# 18 — RBAC / SOD REPORT (Tab Browser)

**Dominio:** Roles (waiter, cashier, chef, admin), segregation of duties  
**Pack fecha:** 2026-08-01  
**Build:** Release deploy `871abc7`

| ID | Escenario | Estado | Notas |
|----|-----------|--------|-------|
| E2E-RBAC-01 | Role smoke waiters/cashier/chef | NOT STARTED | Plan maestro; prior `waiters` soft skip |
| E2E-RBAC-02 | Cashier cannot admin config | NOT STARTED | SoD deep |
| E2E-RBAC-03 | Kitchen cannot open cash | NOT STARTED | SoD deep |
| E2E-RBAC-04 | Cross-role multitab SoD | NOT STARTED | Mandato Tab |
| Prior suite waiters/security | chromium-desktop | Referencia previa | Soft / parcial — no PASS deep SoD este pack |

## Gaps vs mandato

- Ningún caso RBAC/SoD formal en E2ETab nueva  
- Role matrix completa no certificada

**Veredicto dominio RBAC/SoD:** FAIL vs mandato (NOT STARTED en este pack).
