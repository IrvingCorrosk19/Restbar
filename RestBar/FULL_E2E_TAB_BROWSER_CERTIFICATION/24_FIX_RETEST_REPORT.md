# 24 — FIX RETEST REPORT

## BUG-E2E-001

1. Reproducido: E2E-MT-05 timeout en `/Auth/MfaChallenge` (1.3m × 2)  
2. Fix: `Program.cs` rate limit 5→60; `auth.js` retry MFA; delay 1.5s entre contexts  
3. Deploy: commit `871abc7` VPS healthy  
4. Retest: `npx playwright test E2ETab --project=chromium-desktop` → **5 passed (1.9m)**  
5. Log: `FULL_E2E_TAB_BROWSER_CERTIFICATION/e2e-tab-retest.log`

## BUG-E2E-002

Fix de naming evidencia pendiente de re-ejecución MT-05 (no bloquea PASS funcional).
