# GUIDE — Troubleshooting

| Symptom | Check |
|---------|-------|
| Login loop on HTTPS | ForwardedHeaders / cookie Secure / nginx `X-Forwarded-Proto` |
| 500 JSON | CorrelationId + logs; message hidden in prod |
| ModuleDisabled | FeatureFlags |
| Migrate fail on boot | Postgres ready? connection string? |
| SignalR fail | Auth cookie, proxy WebSocket upgrade |
| Health ready unhealthy | `docker exec ... pg_isready`, disk full |

## Administrator Guide (short)

Roles via policies; SuperAdmin for companies; never share admin password; use Audit for investigations.
