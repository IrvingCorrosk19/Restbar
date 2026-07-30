# SECURITY_REPORT.md

| Caso | Resultado |
|------|-----------|
| Anónimo → CashSession | redirect Login PASS |
| Anónimo → Supplier | redirect Login PASS |
| Anónimo → FoodCost | redirect Login PASS |
| Anónimo paid-out API | no HTTP 500 PASS |
| Admin autenticado Cash | acceso PASS |
| Policies CashAccess / PurchasingAccess / CostingAccess / ReportAccess | vigentes en controllers |

**Nota:** pruebas de roles cashier/chef/buyer requieren usuarios sembrados adicionales (backlog SEC-RBAC-MATRIX).

**Veredicto Security suite:** PASS
