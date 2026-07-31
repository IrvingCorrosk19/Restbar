# 14 — Data Leak Report

## Fugas confirmadas y cerradas (RB-1001)

| ID | Vector | Fix |
|----|--------|-----|
| LEAK-01 | `CustomerService.GetAllAsync` unscoped | `ScopedCustomers()` |
| LEAK-02 | SignalR grupos globales | `SignalRTenantGroups` |
| LEAK-03 | Order Details/Edit/GetOrderStatus/Index IDOR | Branch+Company guard |
| LEAK-04 | CashSession Detail/Arqueo/Reconciliation IDOR | `CanAccessCashSession` |
| LEAK-05 | `HasGlobalTenantAccess` superadmin mal precedencia | SuperAdmin siempre global |

## Inventario auditoría (subagente) — residuales

| ID | Vector | Severidad | Estado |
|----|--------|-----------|--------|
| RES-01 | Sin EF global query filters | Medio | Abierto |
| RES-02 | AdvancedReports `branchId` arbitrario (admin) | Medio | Abierto |
| RES-03 | InventoryMovement `branchId` client body | Medio | Abierto |
| RES-04 | CashMovementController sessionId | Medio | Abierto |
| RES-05 | TenantScope helper poco usado en services | Medio | Parcial |
| RES-06 | Lab 3×3×5 / 5000 concurrent / 50 browsers | Alto evidencia | No ejecutado |

Auditoría confirmó: aislamiento claim-driven, no middleware EF; tests browser MT históricamente shallow — deep specs añadidas.

## Política

Cero tolerancia → **no** WORLD CLASS / ENTERPRISE mientras RES-01…03 permanezcan sin lab completo.
