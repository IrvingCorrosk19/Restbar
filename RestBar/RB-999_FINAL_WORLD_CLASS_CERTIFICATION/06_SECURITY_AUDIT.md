# 06 — Security Audit

| Control | Estado | Notas |
|---------|--------|-------|
| Auth cookie + roles | PASS | Rate limit auth |
| RBAC policies | PASS | Analytics/Order/Cash… |
| Multitenant claims | PASS WITH CONDITIONS | Deep IDOR API incompleto |
| CSP / headers | PASS | RB-026; DataTables allowlist |
| Health anonymous | PASS | /health* |
| Secrets in repo | RISK | Deploy scripts históricamente con password — rotar / env |
| Package vulns | PASS High cleared | MailKit/MimeKit **Moderate** residual |
| PCI DSS | **NOT IN SCOPE / FAIL comercial** | Sin procesador; no certificar PCI |
| SOX / audit trails | PARTIAL | Hash chains Cash/FC/Proc/BI; no SOX full |
| ISO 27001 | NOT CERTIFIED | Controles parciales |
| Copilot | OFF | Correcto |
| Destructive BRE actions | Blocked v1 | RB-029 |

## Auditor stance

No declarar compliance PCI/ISO/SOX. Producto usable en piloto con **proceso de pago acordado fuera o adyacente**, HTTPS, backups y least privilege.
