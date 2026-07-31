# 06 — Branch Isolation

| Flujo | Estado | Evidencia |
|-------|--------|-----------|
| Admin branch-scoped users | PASS | UserManagement filtra BranchId |
| Order branch match | PASS | OrderController |
| Cash register por branch | PASS diseño | Cash module |
| Admin puede ver otra branch misma company | Depende rol | admin vs waiter |
| Lab 5 branches × empresa | **NO** | Condición |

`TenantScope.CanAccessBranch` unit tested.
