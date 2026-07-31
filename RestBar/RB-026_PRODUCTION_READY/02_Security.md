# 02 — Security

## Hardening applied in RB-026

| Control | Before | After | Status |
|---------|--------|-------|--------|
| Security headers (CSP, XFO, nosniff, Referrer) | Missing | `SecurityHeadersMiddleware` | **PASS** |
| Correlation ID | Missing | `CorrelationIdMiddleware` | **PASS** |
| ForwardedHeaders | Env only, unused | `UseForwardedHeaders` wired | **PASS** |
| DataProtection keys | Volume unused | PersistKeysToFileSystem | **PASS** |
| Exception message leak | Returned to client | Generic + CorrelationId in prod | **PASS** |
| HSTS / HTTPS redirect | Present (non-Dev) | Unchanged | **PASS WITH CONDITIONS** |
| Cookie HttpOnly + SameSite=Lax | Present | Present; Secure via SameAsRequest / optional Always | **PASS WITH CONDITIONS** |
| Auth rate limit | 5/min prod | Unchanged | **PASS WITH CONDITIONS** |
| CSRF on form POSTs | Partial | Still partial | **PASS WITH CONDITIONS** |
| CSRF on JSON cookie APIs | Missing | Still gap | **FAIL** (residual) |
| Secrets in repo (Dev connection string, deploy.ps1) | Present | Documented; `.env.example` added; backup scripts use env | **FAIL** (residual) |
| Seed in Production | Env-gated Dev-only | OK | **PASS** |
| OWASP Top 10 coverage | Partial | Improved; not complete | **PASS WITH CONDITIONS** |

## Residual risks (must not claim PRODUCTION READY)

1. Cookie-authenticated JSON POSTs without antiforgery (Order AJAX, Payment API).  
2. Tracked secrets historically in `appsettings.Development.json` / deploy scripts — rotate.  
3. SignalR group joins lack tenant/role re-validation.  
4. CSP allows `'unsafe-inline'` (required by current Razor/JS).

**Overall security:** **PASS WITH CONDITIONS**
