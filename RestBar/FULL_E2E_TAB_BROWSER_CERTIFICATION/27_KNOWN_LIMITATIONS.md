# 27 — KNOWN LIMITATIONS

**Pack fecha:** 2026-08-01  
**Cierre pack:** PASS WITH CONDITIONS (`29_*`)

| Limitación | Impacto en cert | Tratamiento |
|------------|-----------------|-------------|
| Copilot disabled in Production | Copilot UI/API ModuleDisabled | **NOT APPLICABLE** |
| Seed endpoints disabled in Production | No HTTP seed en VPS | Seed previo / SQL |
| StockTransfer primarily API | Sin UI completa | API + soft PASS WITH CONDITIONS |
| CashMovement primarily API | Sin UI completa | E2E-CASH-12 API PASS |
| Offline POS SW | Offline resilience | Not deeply certified |
| Nav sin Inventory/Reports en menú | Acceso por URL | Casos usan goto path |
| MFA obligatorio privilegios | Todos admin/manager | TOTP + retries |
| Auth rate limit (histórico) | MT concurrente | FIXED 60/min |
| Firefox / WebKit | No cert primario | Chromium-desktop only |
| Inventory stock delta post-order | No assertion numérica | CONDITION en 29 |
| Full PO create→receive | Residual | CONDITION en 29 |
| Live full POS payment UI | Soft/API primarily | CONDITION en 29 |
| 1 skipped preexistente | Role/seed | Documentado, no FAIL |

No convertir residuales en PASS absoluto.
