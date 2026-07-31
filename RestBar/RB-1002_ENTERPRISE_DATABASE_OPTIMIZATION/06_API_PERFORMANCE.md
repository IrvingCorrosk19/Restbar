# 06 — API Performance (RB-1002)

## Hot APIs

| API surface | Optimization | Notes |
|-------------|--------------|-------|
| Kitchen / StationOrders | EF NoTracking + index | Same JSON/ViewModels |
| Orders list/detail | SplitQuery + NoTracking on Get* | Write endpoints unchanged |
| Payments Get* | NoTracking + SplitQuery | Create/Update still tracked |
| Audit Index | Index on timestamp | Payload still large if unbounded |
| DI executive | Soft-fail (prior) | Not re-tuned this cycle |
| Analytics live/report | Existing SP layer | Relies on analytics schema |

## Payload / serialization

- No contract changes.
- Compression: rely on reverse proxy (nginx) when HTTPS domain used; Kestrel not altered.

## Remaining

- Add pagination DTOs for Audit/Orders GetAll before enterprise scale (Observation O-API-01).
- Avoid duplicate Resolve+query in DI (already soft-catch).
