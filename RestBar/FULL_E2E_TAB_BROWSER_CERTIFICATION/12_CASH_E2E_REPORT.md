# 12 — CASH E2E REPORT (Tab Browser)

**Dominio:** Cash session, register, X/Z, movements  
**Pack fecha:** 2026-08-01  
**Build:** Release deploy `871abc7`

| ID | Escenario | Estado | Notas |
|----|-----------|--------|-------|
| E2E-CASH-01 | Dashboard enabled | NOT STARTED (este pack) | Referencia: RB-010_020_023 cash evidence (otro pack) — no re-run aquí |
| E2E-CASH-02 | Open wizard | NOT STARTED (este pack) | Idem |
| E2E-CASH-03 | Z/X report pages no 500 | NOT STARTED (este pack) | Idem |
| E2E-CASH-04 | Open → ops → close chain multitab | NOT STARTED | Mandato deep E2E |
| E2E-MT-04 | Cash session ajena → deny | NOT STARTED | Hostile MT pendiente |
| CashMovement | API-primary | NOT APPLICABLE (UI deep) | Sin UI completa; API + soft UI only |
| Prior suite `cash*` | chromium-desktop | Referencia previa | 161 PASS baseline — global re-run IN PROGRESS |

## Gaps vs mandato

- Cadena caja deep (apertura → movimientos → X/Z → cierre) no ejecutada en este pack  
- CashMovement: primarily API — no deep browser certification  
- Aislamiento cash cross-tenant: NOT STARTED

**Veredicto dominio Cash:** FAIL vs mandato deep E2E (NOT STARTED en este pack).
