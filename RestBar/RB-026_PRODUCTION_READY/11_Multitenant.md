# 11 — Multitenant

## Controls evidenced

| Control | Evidence | Status |
|---------|----------|--------|
| Company/Branch claims | Auth cookie claims | **PASS** |
| Suspension middleware | `TenantSubscriptionMiddleware` | **PASS** |
| Browser MT smoke | MT-01/02, MT-IDOR soft | **PASS WITH CONDITIONS** |
| SuperAdmin cross-company | `/SuperAdmin` | **PASS WITH CONDITIONS** |
| SignalR group tenancy | Join without re-check | **PASS WITH CONDITIONS** |
| Exhaustive IDOR matrix | Incomplete | **PASS WITH CONDITIONS** |

No cross-tenant data leak demonstrated in certification smoke; deep red-team incomplete.

**Overall multitenant:** **PASS WITH CONDITIONS**
