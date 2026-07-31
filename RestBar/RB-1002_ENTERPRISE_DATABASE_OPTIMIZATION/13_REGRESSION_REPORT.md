# 13 — Regression Report (RB-1002)

## Functional

| Suite | Result |
|-------|--------|
| Unit tests Release | **98/98 PASS** |
| Build Release | **0 errors** |
| Loyalty update path | Fixed to use **tracking** load after GetById became NoTracking |
| Kitchen station filter | Same Pending/Sent + station type semantics; empty boards excluded via SQL Any() |

## Multitenant / security

- No removal of CompanyId/BranchId filters.
- Customer scoping unchanged.
- Kitchen global status query behavior preserved (pre-existing).

## Performance regressions

- First EXPLAIN after index create may show higher planning time (cold cache) — expected.
- AsSplitQuery increases round-trips but reduces row multiplication — acceptable trade.

## API contracts

- Unchanged routes/payloads.
- No pagination introduced on Audit GetAll (would shrink results — deferred).
