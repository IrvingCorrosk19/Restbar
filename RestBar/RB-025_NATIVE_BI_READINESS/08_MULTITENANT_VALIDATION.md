# 08 — Multitenant Validation

## Claim model (evidence)

Authenticated users carry `CompanyId` and `BranchId` claims (see `ExecutiveCommandCenterController`, `BiNativeController`).

Policy `ReportAccess`: roles `admin`, `manager`, `accountant` (`Program.cs`).

## Module gating

| Module | Flag | Isolation pattern |
|--------|------|-------------------|
| Cash | `EnableCashModule` | company_id + branch_id on cash_* |
| Purchasing | `EnablePurchasingModule` | company/branch on POs |
| Food Cost | `EnableFoodCostModule` | company/branch |
| Command Center | `EnableCommandCenter` | snapshot by company+branch |
| Copilot | `EnableCopilot` | conversation.company_id |
| BiNative | ReportAccess (no separate flag) | claims-only tenant |

## BiNative hard rules (implemented)

- Tenant IDs **only from claims**, never from unbound query `companyId`.
- Branch comparison scoped to **claim CompanyId** (all branches of that company).
- Every `sp_*` (except branch comparison) requires both company and branch parameters.

## Known gaps (legacy — still open)

| Area | Issue | Risk |
|------|-------|------|
| `SalesReportService` | Often ignores `BranchId` in core metrics | Cross-branch leak inside same company |
| `AdvancedReportsService` profitability | Date-only filter historically | Same-company leak |
| User entity | No `CompanyId` column; via `Branch` | OK if branch always set |
| Column naming | PascalCase vs snake_case | Wrong SQL = empty/wrong rows if unquoted |

## Validation checklist

- [x] BiNative endpoints use claim CompanyId/BranchId
- [x] SPs filter tenant columns
- [ ] Legacy SalesReport BranchId filter (gap — track in 09)
- [ ] Automated MT test for BiNative cross-company denial (recommended follow-up)

## Verdict

**Native BI path (BiNative + enterprise dashboards) is tenant-safe by design.**  
**Legacy report services remain PARTIAL** and must not be the sole BI surface for multi-branch enterprises until fixed.
