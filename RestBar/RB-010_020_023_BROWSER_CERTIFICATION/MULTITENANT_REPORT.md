# MULTITENANT_REPORT.md

| Caso | Resultado |
|------|-----------|
| MT-01 admin@restbar.com sesión tenant | PASS |
| MT-02 admin@costa.restbar.com | SKIP (no seeded en DB actual) |

Aislamiento cross-company profundo (Company A no ve datos B) requiere seed 3-companies + asserts de datos — pack previo `FUNCTIONAL_CERTIFICATION_3_COMPANIES` / PKS. Esta corrida valida smoke de sesión tenant.

**Veredicto:** PASS parcial (smoke). Cross-tenant data isolation = backlog / reutilizar pack TC3.
