# 04 — Database Audit

## Modelo

- Entidades operativas llevan `CompanyId` / `BranchId` (Order, Table, Customer, Product, Cash*, etc.).
- **No** hay Global Query Filter EF automático por tenant en `RestBarContext` — el aislamiento depende de servicios/controllers.

## Riesgo residual

| Hallazgo | Severidad | Estado |
|----------|-----------|--------|
| Sin global filter EF | Medio | Condición — callers deben filtrar |
| Seed ThreeCompanies | Bajo | 3 empresas / 1 branch (no 3×3×5) |
| analytics SPs | Medio | Deben recibir company/branch params (patrón analytics) |

## Consultas auditadas (muestra)

- Order list/station: filtros Branch/Company en controller.
- Customer: **antes** `ToListAsync()` global → **ahora** `ScopedCustomers()`.
- SignalR ya no implica SQL leak; fan-out era en hub.

Auditoría SQL exhaustiva de **todas** las queries: **no completada** → condición.
