# GUIDE — Monitoring

| Signal | Where |
|--------|-------|
| Process up | `/health/live`, docker health |
| DB up | `/health/ready` |
| App errors | `docker logs`, `/Audit` |
| Request trace | `X-Correlation-ID` |
| Perf UX | Playwright PERF budgets / browser timings |

Future: Serilog → Seq/ELK, Prometheus counters — **not shipped**.
