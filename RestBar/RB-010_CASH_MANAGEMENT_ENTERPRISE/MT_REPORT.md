# RB-010 — MULTITENANCY REPORT (MT)

**Fecha:** 2026-07-29

---

## Modelo

- CashRegister → CompanyId, BranchId (required)
- CashSession → CompanyId, BranchId (required, denormalized from register)
- CashMovement → CompanyId, BranchId (required, denormalized from session)
- CashAuditEvent → CompanyId, BranchId

## Queries

- Dashboard: filtrado por `BranchId` del claim
- Register list: filtrado por `BranchId`
- Active session: filtrado por `userId` + `branchId`

## Payment linkage

- `Payment.CashSessionId` nullable — pagos históricos sin sesión
- Hook asigna sesión solo del mismo branch vía sesión activa del cajero

## Tests existentes

- TenantScopeTests: 10/10 PASS (sin regresión)

**MT isolation:** ✅ diseño conforme · ⏳ test E2E cross-tenant pendiente UAT
