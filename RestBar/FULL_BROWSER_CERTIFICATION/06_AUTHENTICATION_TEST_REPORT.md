# 06 — AUTHENTICATION TEST REPORT

**Fecha:** 2026-07-30 · **Target:** VPS:8084 · **Browser:** Chromium desktop

| Test ID | Resultado | Notas |
|---------|-----------|-------|
| SMK-01 | PASS | Login admin |
| SMK-02 | PASS | Redirect protegido |
| AUTH-01 | PASS | Password inválido |
| AUTH-02 | PASS | Logout vía dropdown (fix test: abrir #userDropdown) |
| AUTH-03 | PASS | ForgotPassword &lt;500 |
| AUTH-04 | PASS | Profile |
| AUTH-05 | PASS | Contexts aislados |
| SEC-01..05 | PASS | AuthZ gates |

**Defectos:** BUG-001 AUTH-02 — fallo de automatización (botón logout oculto en dropdown). **FIXED** — no es defecto de app.
