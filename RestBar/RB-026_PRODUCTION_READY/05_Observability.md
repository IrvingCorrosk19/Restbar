# 05 — Observability

## Implemented (RB-026)

| Capability | Endpoint / component | Status |
|------------|----------------------|--------|
| Liveness | `GET /health/live` | **PASS** |
| Readiness (Postgres) | `GET /health/ready` | **PASS** |
| Aggregate health | `GET /health` | **PASS** |
| Correlation ID | `X-Correlation-ID` header + log scope | **PASS** |
| Audit logging | `AuditMiddleware` + `IAuditLogService` | **PASS WITH CONDITIONS** |
| Structured Serilog/OTel | Not present | **FAIL** |
| Metrics Prometheus | Not present | **FAIL** |
| Central alert dashboard | Not present | **FAIL** |
| Slow query auto-detect | Not present | **PASS WITH CONDITIONS** |

**Overall observability:** **PASS WITH CONDITIONS**
