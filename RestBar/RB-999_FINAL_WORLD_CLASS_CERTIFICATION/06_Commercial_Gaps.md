# 06 — Commercial Gaps

Solo brechas que **impiden o dificultan vender**. No wishlist técnica.

| ID | Brecha | Clase | Impide venta? | Notas |
|----|--------|-------|---------------|-------|
| G1 | Sin procesador de pagos / PCI story | **Crítica** | Sí vs Toast/Square | Competidores ganan por defecto |
| G2 | Sin offline POS | **Crítica** | Sí en muchos locales | Red inestable = deal-breaker |
| G3 | Sin modelo comercial (SKU, contrato, billing) | **Crítica** | Sí self-serve | Journey paso 2 roto |
| G4 | Sin red de soporte/implementación | **Crítica** | Sí a escala | Solo venta asistida |
| G5 | Onboarding sin wizard | **Alta** | Retrasa go-live | Fricción, no feature nueva de negocio |
| G6 | App móvil nativa | **Alta** | Pierde vs Toast | Web responsive mitiga parcial |
| G7 | Marketplace integraciones | **Alta** | Cadenas/contabilidad | R365/Toast ganan |
| G8 | CSRF JSON / secretos históricos | **Alta** (confianza) | Enterprise security review | RB-026 residual |
| G9 | Load 1k–5k no evidenciado | **Media** | Cadenas grandes | Piloto OK |
| G10 | Conteos físicos / WMS | **Media** | Ops inventario estricto | Inventario actual usable |
| G11 | Reports ExportPdf stub | **Baja** | No | Mitigado Analytics/AdvancedReports |
| G12 | Dark Mode / i18n / loyalty / reservas | **Cosmética / Baja** | Segmento-specific | No bloquear piloto mid-market |

**Regla:** no construir loyalty/reservas/hotel antes de G1–G5.
