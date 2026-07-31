# 03 — API Report

| Área | Aislamiento | Evidencia |
|------|-------------|-----------|
| Decision Intelligence / Business Rules APIs | Exigen `CompanyId` claim | Forbid si null |
| OrderController | Valida table/order Branch+Company | `CanAccessTable` / branch match |
| Customer APIs vía service | **Scoped post-fix** | `CustomerService.ScopedCustomers` |
| Manipulación JWT/headers exhaustiva | **No lab completo** | Condición |
| Horizontal privilege escalation all endpoints | **Parcial** | Condición |

Unit: **98 PASS** (incluye TenantScope + SignalRTenantGroups).
