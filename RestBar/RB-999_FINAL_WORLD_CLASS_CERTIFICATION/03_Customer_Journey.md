# 03 — Customer Journey

Simulación del recorrido comercial → primer valor. Fricciones = evidencia de producto/ops.

| # | Paso | Estado actual | Fricción | Severidad |
|---|------|---------------|----------|-----------|
| 1 | Demo | Manual (VPS/login admin) | No portal self-serve demo | Media |
| 2 | Compra licencia | **No existe** billing/SKU | Bloquea venta self-serve | **Crítica** |
| 3 | Instalación | Docker + guías RB-026 | Requiere DevOps o partner | Alta |
| 4 | Config inicial | Company/Branch/Areas/Tables/Stations/Products | Muchos clics; sin wizard onboarding | Alta |
| 5 | Capacitación | Sin LMS; UI en español | Depende de implementador | Media |
| 6 | Primer día | POS + KDS operativos | Dependencia red; sin offline | Alta |
| 7 | Primera venta | Flujo mesa→producto→cocina→pago evidenciado | Pago tender UI incompleto vs competencia | Alta |
| 8 | Primer cierre | CashSession arqueo/X/Z | Requiere módulo caja ON + training | Media |
| 9 | Primer inventario | Inventory index + movements | Sin conteo físico | Media |
| 10 | Primera compra | Supplier + PO | Receive E2E profundo incompleto | Media |
| 11 | Primer reporte | AdvancedReports / ExecutiveAnalytics | Reports ExportPdf stub — usar Analytics | Baja (mitigado) |
| 12 | Primer dashboard | `/ExecutiveAnalytics` | OK | — |
| 13 | Primera auditoría | `/Audit` + cash/PO audits | OK | — |

## Tiempo estimado a valor (piloto asistido)

| Hito | Estimación realista |
|------|---------------------|
| Infra Docker lista | 0.5–2 días (con partner) |
| Catálogo + mesas + estaciones | 1–3 días |
| Primer servicio completo | Día 1–2 post-config |
| Caja + inventario confiables | Semana 1–2 con disciplina |

**Sin partner de implementación, el journey se rompe en pasos 2–4.**
