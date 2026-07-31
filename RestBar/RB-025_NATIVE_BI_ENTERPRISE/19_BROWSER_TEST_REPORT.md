# 19 — Browser Test Report

**Fecha:** 2026-07-30  
**Target:** `http://164.68.99.83:8084`  
**Suite:** `tests/Browser/Analytics/analytics.spec.js` (chromium-desktop)

| ID | Caso | Resultado |
|----|------|-----------|
| AN-01 | Centro Ejecutivo loads | **PASS** |
| AN-02 | Live KPIs JSON | **PASS** |
| AN-03 | Tabs Rendimiento / Decisiones | **PASS** |
| AN-04 | Report shell + ReportData (5 keys) | **PASS** |
| AN-05 | Export CSV + XLSX | **PASS** |
| AN-06 | Unauthenticated redirect | **PASS** |

**Totales:** 6/6 PASS  
**HTTP smoke:** `scripts/Run-AnalyticsSmoke.ps1` → PASS=7 FAIL=0  
**Evidencia:** `RB-025_NATIVE_BI_ENTERPRISE/evidence/`
