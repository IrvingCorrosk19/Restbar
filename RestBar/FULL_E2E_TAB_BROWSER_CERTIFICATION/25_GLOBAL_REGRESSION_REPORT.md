# 25 — GLOBAL REGRESSION REPORT

**Corrida:** 2026-08-01 · VPS `http://164.68.99.83:8084` · chromium-desktop  
**Resultado:** **179 passed / 0 failed / 1 skipped** (18.2m)

Log: `logs/global-regression-20260801-rerun.log`

## Includes

- Suite E2ETab completa (18 casos: deep cash/pay/inv/PO/FC/BI/admin/RBAC/hostile/UX/auth + MT + POS/KDS)
- Smoke / Auth / Orders / Cash / Payments / Inventory / Procurement / FoodCost / BI / Admin / Security / Responsive

## Prior notes

- CASH-Z-01 blank 404 → fixed `ReportMissing` (`d99b45e`); retained green in this run  
- Auth rate-limit Prod 60/min supports concurrent MFA contexts

## Verdict

**GLOBAL REGRESSION: PASS**
