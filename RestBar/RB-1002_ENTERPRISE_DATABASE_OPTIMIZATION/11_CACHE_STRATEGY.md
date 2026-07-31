# 11 — Cache Strategy (RB-1002)

## Present

| Layer | Usage | Tenant safety |
|-------|-------|---------------|
| `IMemoryCache` | Password reset tokens | Keyed by token; not cross-tenant data |
| Distributed cache | **None** | N/A |
| Response cache | **None** | N/A |
| EF 2nd-level | **None** | N/A |

## Decisions this cycle

- **No new caches** introduced — avoids accidental cross-tenant leakage.
- Prefer index + NoTracking over caching KPI payloads until explicit tenant-keyed cache policy exists.

## Future (O-CACHE-01)

- Tenant-keyed `IMemoryCache` for AnalyticsLiveSnapshot with short TTL (5–15s) and CompanyId+BranchId key.
- Never cache RBAC decisions longer than request without invalidation hook.
