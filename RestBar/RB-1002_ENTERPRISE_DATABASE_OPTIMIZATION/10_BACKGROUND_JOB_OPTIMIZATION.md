# 10 — Background Job Optimization (RB-1002)

## Inventory

- No dedicated Hangfire/Quartz host found in core app.
- Forecast persistence runs **inline** on DI API/cockpit (`persistRun: true`).
- BI/analytics SPs invoked on demand.

## Actions

- Soft-fail already prevents cockpit 500 storms.
- Indexes support on-demand report/forecast history queries (`di_*` already indexed).

## Recommendations (not implemented)

- Move forecast persist to queued background worker to keep API P95 low under concurrent executives.
- Materialized views for executive-summary only after KPI parity tests.
