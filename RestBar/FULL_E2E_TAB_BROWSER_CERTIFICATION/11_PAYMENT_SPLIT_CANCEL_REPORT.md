# 11 — PAYMENT / SPLIT / CANCEL REPORT (Tab Browser)

**Dominio:** Payment API, PaymentView, person split, cancelaciones  
**Pack fecha:** 2026-08-01  
**Build:** Release deploy `871abc7`

| ID | Escenario | Estado | Notas |
|----|-----------|--------|-------|
| E2E-PAY-01 | Payment summary / partial API shape | NOT STARTED | Plan maestro; no corrido en E2ETab |
| E2E-PAY-02 | GetPaymentHistory tenant guard | NOT STARTED | Hostile payment ID pendiente |
| E2E-PAY-03 | Pago mixto (cash + card) | NOT STARTED | Mandato deep E2E |
| E2E-PAY-04 | Split por persona | NOT STARTED | Mandato deep E2E |
| E2E-PAY-05 | Cancelación parcial / void | NOT STARTED | Mandato deep E2E |
| Prior suite payments/security | chromium-desktop | Referencia previa | Suite previa 161 PASS — no sustituye deep Tab pack |

## Gaps vs mandato

- Ningún caso Payment ejecutado en la suite E2ETab nueva  
- Split / cancel / pago mixto: NOT STARTED  
- IDOR payment GUID ajeno: pendiente en hostile MT ampliado

**Veredicto dominio Payment:** FAIL vs mandato deep E2E (NOT STARTED en este pack).
