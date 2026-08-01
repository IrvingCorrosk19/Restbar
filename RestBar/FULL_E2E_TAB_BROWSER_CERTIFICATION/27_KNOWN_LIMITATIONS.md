# 27 — KNOWN LIMITATIONS

**Pack fecha:** 2026-08-01  
**Build:** `871abc7`

| Limitación | Impacto en cert | Tratamiento |
|------------|-----------------|-------------|
| Copilot disabled in Production | Copilot UI/API ModuleDisabled | **NOT APPLICABLE** — no FAIL |
| Seed endpoints disabled in Production | No HTTP seed en VPS | Seed previo / Dev gate / SQL |
| StockTransfer primarily API | Sin UI completa browser | API + soft; no deep UI PASS |
| CashMovement primarily API | Sin UI completa browser | API + soft; no deep UI PASS |
| Offline POS SW implemented | Offline resilience | **Not deeply certified this pack** |
| Nav sin Inventory/Reports en menú principal | Acceso por URL directa | Casos prior suite usan goto path |
| MFA obligatorio privilegios | Todos admin/manager | `completeMfaIfNeeded` + TOTP seed |
| Auth rate limit (pre-fix) | MT concurrente fallaba | FIXED 5→60 (`871abc7`); RETEST PASS |
| Firefox / WebKit | No cert primario | Chromium-desktop only este pack |
| Global regression re-run | Cierre pack incompleto | **IN PROGRESS** |
| Deep E2E cash/inv/PO/FC/BI chains | Mandato incompleto | Documentado FAIL / NOT STARTED |
| Hostile MT full ID surface | Cobertura parcial | E2E-MT-02/05 PASS; resto pendiente |

No usar limitaciones para convertir NOT STARTED en PASS.
