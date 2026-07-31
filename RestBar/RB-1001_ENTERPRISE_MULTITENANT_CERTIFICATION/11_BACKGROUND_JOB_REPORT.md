# 11 — Background Job Report

| Job / proceso | Tenant scope | Estado |
|-------------|--------------|--------|
| DI forecast runs | Persistidos con company/branch | Diseño COND |
| Business rules evaluate | CompanyId required | COND |
| Analytics snapshots/SPs | Params company/branch | COND |
| Job scheduler distribuido multi-tenant | No formal | Condición |

No se detectó un hosted service que itere tenants sin filtro en esta auditoría puntual; **certificación formal de todos los jobs: pendiente**.
