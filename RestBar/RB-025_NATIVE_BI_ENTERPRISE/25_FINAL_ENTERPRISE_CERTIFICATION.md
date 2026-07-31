# 25 — Final Enterprise Certification

## Answers

1. ¿BI nativo funcional? **SÍ (con condiciones)** — Centro Ejecutivo + analytics schema + exports.
2. ¿Decisiones reales? margen negativo, caja, stock, PO atrasadas, caída ventas, demoras cocina.
3. ¿KPIs disponibles? ver KpiCatalog Available / AvailableWithLimitations.
4. ¿KPIs no calculables? TAX, GUESTS, RESERVED, WAREHOUSE, COUNT_ACC, etc.
5. ¿Calidad? ver 04 — costos nulos/dualidad y timestamps opcionales.
6. ¿Consistencia operación? misma fuente OLTP; single path executive via sp_executive_dashboard.
7. ¿Rendimiento? no medido a escala → condición.
8. ¿MT completo en módulo nuevo? SÍ; legacy reports aparte.
9. ¿Sin Power BI? SÍ.
10. ¿Prod-ready? **PASS WITH CONDITIONS** tras migrar + smoke + regresión.

## Result: PASS WITH CONDITIONS

Conditions: FC snapshots ops for Food Cost %, QuestPDF/native PDF binary optional (HTML print OK), 1M-row lab deferred off-prod. Playwright analytics **PASS** 6/6; EXPLAIN on VPS &lt;10 ms.
