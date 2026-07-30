# 07 — PERMISSION MATRIX

**Policy base:** `CashAccess` = admin, manager, supervisor, cashier, accountant

Permisos adicionales (strings en `AuthService.HasPermissionAsync`):

| Permission | Roles default | Descripción |
|------------|---------------|-------------|
| `cash.register.view` | manager+ | Ver registers |
| `cash.register.manage` | admin, manager | CRUD registers |
| `cash.session.open` | cashier+ | Abrir sesión |
| `cash.session.close` | cashier+ | Cerrar propia sesión |
| `cash.session.close.any` | supervisor+ | Cerrar sesión ajena |
| `cash.session.reopen` | manager+ | Reapertura |
| `cash.movement.paidin` | cashier+ | Ingreso efectivo |
| `cash.movement.paidout` | cashier+ (< threshold) | Retiro |
| `cash.movement.paidout.approve` | supervisor+ | Retiro alto |
| `cash.count.perform` | cashier+ | Arqueo |
| `cash.count.witness` | supervisor+ | Testigo arqueo |
| `cash.approval.variance` | supervisor+ | Aprobar diferencia |
| `cash.report.z.view` | cashier+ (own), supervisor+ (branch) | Z report |
| `cash.report.z.export` | manager, accountant | Export |
| `cash.audit.view` | manager, accountant, admin | Forense |
| `cash.incident.manage` | supervisor+ | Resolver incidentes |
| `cash.override.supervisor` | supervisor+ | Overrides POS void/refund |
| `cash.holding.view` | admin, superadmin | Multi-branch |

---

# API → Permission map (resumen)

| Endpoint | Permission |
|----------|------------|
| POST /api/cash/sessions/open | cash.session.open |
| POST /api/cash/sessions/{id}/close | cash.session.close |
| POST /api/cash/movements/paid-out | cash.movement.paidout (+ approval si > limit) |
| POST /api/cash/approvals/{id}/approve | cash.approval.variance |
| GET /api/cash/reports/z/{sessionId} | cash.report.z.view |

Accountant: read-only cash reports; no open/close.

Waiter/chef: sin cash permissions unless also cashier role.

---

# Multitenant enforcement

Every permission check **after** `TenantScope.EnsureCompanyAccess` / `EnsureBranchAccess`.

SuperAdmin: cross-company read for holding reports only; no mutate cash without impersonation audit.
