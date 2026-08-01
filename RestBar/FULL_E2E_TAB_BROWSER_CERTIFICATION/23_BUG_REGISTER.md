# 23 — BUG REGISTER

| Bug ID | Test ID | Módulo | Severidad | Hallazgo | Causa raíz | Corrección | Estado |
|--------|---------|--------|-----------|----------|------------|------------|--------|
| BUG-E2E-001 | E2E-MT-05 | Auth/RateLimit | P1 | MFA challenge timeout al login concurrente 3 tenants | `auth_endpoints` PermitLimit=5/min en Production | Subir a 60/min + cola 10; retry MFA en auth.js; delay entre logins | FIXED / RETEST PASS |
| BUG-E2E-003 | CASH-Z-01 | CashReport | P2 | `/CashReport/ZReport` sin sessionId → 404 vacío (assertion body falla) | Access guard NotFound sin UX | View `ReportMissing` con mensaje Sesión/Z | FIXED |

No P0 abiertos en esta corrida Tab Browser.
