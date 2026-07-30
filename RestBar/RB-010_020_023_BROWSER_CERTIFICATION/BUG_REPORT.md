# BUG_REPORT — hallados por browser certification

| ID | Severidad | Hallazgo | Fix |
|----|-----------|----------|-----|
| BUG-CERT-001 | Critical | Ruta `{action=Login}` → 404 en `/Supplier` etc. | `Program.cs` Index + `/`→Login |
| BUG-CERT-002 | High | ModuleDisabled vistas ausentes | Vistas añadidas |
| BUG-CERT-003 | Medium | XReport.cshtml ausente | Vista XReport |
| BUG-CERT-004 | High | `wwwroot` vacío (CSS/JS/favicon 404) | Restaurado desde git |
| BUG-CERT-005 | High | Acciones sin Feature Flag | Gates Cash/PO/Supplier/FC/Recipe |
| BUG-CERT-006 | Medium | Create PO sin validar líneas | Validación fail-closed |
| BUG-CERT-007 | Low | OpenWizard sin registers | Auto-crea CAJA-1 |

## Retest

- Build: **0 errors**
- Unit: **69/69 PASS**
- Browser desktop: **41 PASS / 1 skip / 0 FAIL**
- Static: favicon.ico, site.css, logo.png → **200**
