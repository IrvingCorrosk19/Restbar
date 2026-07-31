# 02 — Functional Coverage

**Pregunta:** ¿Hay necesidad operativa moderna que RestBar no pueda resolver?

**Respuesta:** **Sí hay gaps materiales** (pagos procesados, offline, nómina, WMS/conteo físico, guest covers, CRM RFM completo). El núcleo POS→KDS→Caja→Inv→Compras→FC→BI **sí** cubre un piloto multi-sucursal online.

## Clasificación por módulo

| Módulo | Nota | Justificación |
|--------|------|---------------|
| POS / Orders | **Bueno** | Flujo completo + browser/concurrency; sin unit profunda OrderService |
| KDS / Kitchen | **Bueno** | Timestamps parciales; usable |
| Caja | **Excelente** | SM + hash + browser amplio + reportes X/Z |
| Inventario | **Bueno** | Movimientos/cobertura; sin conteo físico enterprise |
| Compras | **Bueno** | PO/PR/GR + scores; dual approval hardcode 500 |
| Food Cost | **Bueno** | Snapshots + menu eng; depende de generación de snapshots |
| Reportes / Exports | **Aceptable** | HTML/CSV/XLSX; PDF nativo deferred; TODOs históricos |
| BI Nativo | **Bueno** | Schema analytics + SPs + KPI catalog |
| Decision Intelligence | **Aceptable** | Forecast estadístico + recs; DQ 68; PILOT |
| Forecast | **Aceptable** | Backtest unit; no ARIMA/ML; accuracy prod no multi-branch cert |
| Automatización (Rules) | **Aceptable** | Motor v1 + plantillas; sin scheduler distribuido |
| Seguridad / RBAC | **Bueno** | Policies + middleware; CSRF JSON residual |
| Multiempresa / MT | **Bueno** | Company→Branch; IDOR deep parcial |
| Configuración | **Bueno** | Flags + settings |
| Pagos / PCI | **Crítico** (gap comercial) | Tip/split sí; **sin** procesador / offline / PCI DSS scope |
| Offline POS | **Crítico** (gap) | No existe |
| Workforce / nómina | **Débil** | Shifts sin wage |
| Clientes / CRM | **Débil** | Entity básica; RFM deferred |
| Copilot | **N/A (off)** | Flag false — correcto hasta APIs certificadas |

## Necesidades no cubiertas (restaurante moderno)

1. Cobro con terminal / gateway + liquidación.  
2. Operación sin red (offline queue).  
3. Conteo físico cíclico WMS.  
4. Labor cost % y scheduling avanzado.  
5. Marketplace / loyalty / marketing automation.  
6. Hiperescala 1k+ locales concurrentes (no evidenciado).
