# 25 — BUG REGISTER

| Bug ID | Test ID | Módulo | Descripción | Severidad | Causa raíz | Corrección | Prueba regresión | Estado |
|--------|---------|--------|-------------|-----------|------------|------------|------------------|--------|
| BUG-001 | AUTH-02 | Auth/Test | Logout button no visible sin abrir dropdown | P3 (test) | Locator sin abrir `#userDropdown` | Abrir dropdown + `.dropdown-menu.show button.logout-btn` | AUTH-02 retest PASS | **FIXED** |
| BUG-002 | POS-CONC-01 | Orders/Test | Timeout 90s al navegar KDS en 2º context | P3 (flaky test) | Orden lento mesa+send antes de kitchen goto | Kitchen primero + timeout 120s | POS-CONC-01 PASS | **FIXED** |
| BUG-003 | RPT-04 | Reports | ExportPdf/Excel clásicos stub | P2 | TODO histórico ReportsController | Documentado; AdvancedReports+Analytics OK | RPT-04 soft PASS | **OPEN (known stub)** |
| BUG-004 | — | Email | EmailController sin Views/Email | P3 | Vista faltante | Pendiente UI | ADM-08 soft | **OPEN** |
| BUG-005 | — | Payment | Views/Payment huérfana | P4 | Duplicado vs PaymentView | Limpieza futura | — | **OPEN** |

**P0 abiertos:** 0  
**P1 abiertos:** 0  
**P2 abiertos:** 1 (stub Reports export — mitigado por otros exporters)
