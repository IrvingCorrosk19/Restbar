# 13 — Support

## Support mode toolkit

| Tool | Use |
|------|-----|
| `/health`, `/health/live`, `/health/ready` | Instant status |
| `X-Correlation-ID` response header | Tie user report to logs |
| `/Audit` UI | Manager+ audit trail |
| `docker logs restbar_web` | Runtime errors |
| Feature flags | Disable broken module without full rollback |

## Incident checklist

1. Capture CorrelationId / time / tenant / branch / user.  
2. Check `/health/ready`.  
3. Check postgres disk / connections.  
4. Reproduce with admin on same branch.  
5. If data risk: backup before fix.  
6. Post-incident: update `25` style bug register.

No dedicated “support impersonation” mode — use SuperAdmin carefully with audit.

**Overall support:** **PASS WITH CONDITIONS**
