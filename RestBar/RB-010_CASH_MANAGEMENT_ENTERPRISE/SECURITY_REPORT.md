# RB-010 — SECURITY REPORT

**Fecha:** 2026-07-29

---

## Tenant isolation

- Todas las entidades cash incluyen `CompanyId` + `BranchId`
- Controllers usan claims `CompanyId` / `BranchId` del usuario autenticado
- `TenantScope` foundation sin cambios; compatible con filtros futuros RB-089

## RBAC

| Policy | Roles |
|--------|-------|
| CashAccess | admin, manager, cashier, accountant, supervisor |

## Integridad

| Control | Implementación |
|---------|----------------|
| Hash chain movements | SHA-256 encadenado por sesión |
| Audit events hash | SHA-256 encadenado |
| Z report integrity hash | SHA-256 sobre JSON snapshot |
| Idempotency | `CashMovement.IdempotencyKey` UNIQUE |
| RowVersion | `CashSession.RowVersion` optimistic concurrency |

## Dual approval

- `CashApprovalService.RequiresDualApprovalAsync` para paid-out grande, varianza, reopen
- API paid-out retorna `202 Accepted` con `approvalId` cuando requiere aprobación

## Feature flag

- `EnableCashModule` default **false** — superficie no expuesta en producción hasta UAT

**Hallazgos críticos:** 0
