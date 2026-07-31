# 01 — Test Matrix

| Módulo | Funcional | Browser | API | Seguridad | Multitenant | Empresa | Sucursal | Piso | PASS |
|--------|-----------|---------|-----|-----------|-------------|---------|----------|------|------|
| Auth | P | P parcial | — | P | P claims | — | — | — | COND |
| Users/RBAC | P | admin | — | P parcial | P | P | P | — | COND |
| Companies/Branches | P | admin | — | P | P | P | P | — | COND |
| Areas/Floors | P | floors | — | — | P vía Area | — | P | P | COND |
| Tables/Stations | P | tables/stations | — | — | P | — | P | P | COND |
| POS/Orders | P | orders | soft IDOR | P parcial | P OrderController | — | P | — | COND |
| KDS | P | kitchen | — | — | **SignalR scoped** | — | P | — | COND |
| Customers | P | MT-D03 | — | **Fixed scope** | **PASS fix** | P | — | — | PASS* |
| Payments/Cash | P | cash/payments | — | — | P flags | — | P | — | COND |
| Inventory/PO | P | inv/proc | — | — | P | — | P | — | COND |
| Reports/BI/DI | P | reports/analytics | claim gate | — | filter claims | P | P | — | COND |
| SignalR | — | — | — | **Fixed groups** | **PASS fix** | P | P | — | PASS* |
| Full 3×3×5 lab | — | — | — | — | **NOT RUN** | — | — | — | **FAIL scale** |

\*PASS del fix unitario/código; no lab multi-browser 50 clientes.
