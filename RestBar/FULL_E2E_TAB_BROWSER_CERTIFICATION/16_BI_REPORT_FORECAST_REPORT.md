# 16 — BI / REPORT / FORECAST REPORT (Tab Browser)

**Dominio:** ExecutiveAnalytics, BiNative, CommandCenter, Reports, Forecast, DI  
**Pack fecha:** 2026-08-01  
**Build:** Release deploy `871abc7`

| ID | Escenario | Estado | Notas |
|----|-----------|--------|-------|
| E2E-BI-01 | Executive + DI soft | NOT STARTED (este pack) | Referencia: prior analytics/DI / RB-025 — no re-run aquí |
| E2E-RPT-01 | Reports + Advanced export | NOT STARTED | — |
| E2E-BI-02 | Forecast pages / math UI | NOT STARTED | Unit Forecast histórico ≠ browser E2E |
| E2E-MT-03 | Report filters no filtran otro CompanyId | NOT STARTED | Hostile MT pendiente |
| Copilot | Production disabled | **NOT APPLICABLE** | Copilot flag **off** en Production |
| Prior suite analytics/reports | chromium-desktop | Referencia previa | 161 PASS baseline — global re-run IN PROGRESS |

## Gaps vs mandato

- Deep BI chain post-ventas reales: NOT STARTED  
- Forecast/export isolation deep SQL cross-check: pendiente (`19_*`)  
- Copilot: intentionally N/A in Production

**Veredicto dominio BI/Reports/Forecast:** FAIL vs mandato deep E2E (NOT STARTED; Copilot N/A).
