# 05 — RBAC Report

| Control | Estado |
|---------|--------|
| Roles enum + policies Program.cs | PASS |
| SuperAdmin bypass Company | By design |
| TenantScope.CanAccessCompany | Unit PASS |
| Escalada vertical exhaustiva | Parcial |
| SoD formal | GAP (no bloquea MT company isolation) |
| URL directa / GUID manip | Soft tests PASS (≠500) |

Claims en cookie: `CompanyId`, `BranchId`, `UserId`, `UserRole`.
