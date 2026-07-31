# 08 — Report Validation

| Reporte | Filtro tenant | Estado |
|---------|---------------|--------|
| Sales Reports | ResolveBranchId | COND |
| AdvancedReports | filters BranchId | COND |
| Cash X/Z | Session/Register scope | COND |
| BI / Executive / DI | CompanyId+BranchId claims | COND |
| Mezcla demostrada en lab 3 tenants | **NO** | Condición |

Diseño: filtros por claim; **no** hay prueba automatizada que compare totales PG cross-tenant en esta corrida.
