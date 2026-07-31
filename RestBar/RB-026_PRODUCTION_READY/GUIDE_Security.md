# GUIDE — Security

1. Rotate any password ever committed to git.  
2. Use `.env` not committed; `.env.example` only.  
3. Terminate TLS at nginx; enable ForwardedHeaders (wired).  
4. When HTTPS-only: `"Security": { "RequireSecureCookies": true }`.  
5. Keep auth rate limit in Production.  
6. Prefer antiforgery on new cookie POSTs; plan token API for SPA later.  
7. Review CSP CDN allowlist when adding scripts.
