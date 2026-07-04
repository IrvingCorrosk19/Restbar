# ORDER FUNCTIONAL CERTIFICATION — RestBar

Certificación funcional enterprise del módulo **Order/Index**.

## Veredicto

**ORDER ENTERPRISE FUNCTIONAL CERTIFICATION: PASS** (119/119 tests)

## Ejecutar certificación

```powershell
powershell -File scripts\Run-AllCertifications.ps1
```

## Entregables (prompt master)

| # | Documento |
|---|-----------|
| 01 | `01_MASTER_TEST_PLAN.md` |
| 02 | `02_BUSINESS_SCENARIOS.md` |
| 03 | `03_ENTERPRISE_TEST_CASES.md` |
| 04 | `04_EXECUTED_TESTS.md` |
| 05 | `05_DEFECT_LOG.md` |
| 06 | `06_ROOT_CAUSE_ANALYSIS.md` |
| 07 | `07_FIXES_APPLIED.md` |
| 08 | `08_RETEST_RESULTS.md` |
| 09 | `09_CONCURRENCY_REPORT.md` |
| 10 | `10_MULTITENANT_REPORT.md` |
| 11 | `11_KITCHEN_ROUTING_REPORT.md` |
| 12 | `12_BAR_ROUTING_REPORT.md` |
| 13 | `13_INVENTORY_VALIDATION.md` |
| 14 | `14_FINANCIAL_VALIDATION.md` |
| 15 | `15_AUDIT_REPORT.md` |
| 16 | `16_SIGNALR_REPORT.md` |
| 17 | `17_USABILITY_REPORT.md` |
| 18 | `18_PRODUCTION_READINESS.md` |
| 19 | `19_FUNCTIONAL_CERTIFICATION_REPORT.md` |
| 20 | `20_EXECUTIVE_SUMMARY.md` |

## Backup

`backups/RestBar_pre_order_enterprise_20260704_171831.dump`

## Scripts

- `scripts/Run-OrderCertification.ps1` — 38 casos Order/Index
- `../scripts/Run-AllCertifications.ps1` — batería completa
- `../FUNCTIONAL_CERTIFICATION/scripts/Cert-Common.ps1` — reset entre suites

## Docs legacy

Archivos `*_ORDER_*` conservados como referencia histórica; usar prefijo `01_`–`20_` sin `ORDER_`.
