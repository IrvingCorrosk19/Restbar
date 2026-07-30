# 03 — DOMAIN MODEL

| Entidad | Acción | Justificación |
|---------|--------|---------------|
| ExecutiveSnapshot | CREAR | Read model persistible del CC |
| BiInsight | CREAR | Explicaciones accionables |
| BiAlert | CREAR | Alertas cross-módulo |
| BiScore | CREAR | Scores Branch/Product/Supplier cache |
| BiAuditEvent | CREAR | Quién consultó qué |
| ForecastSeed | CREAR | Prep predictivo (sin IA) |
| FoodCostSnapshot / Cash / PO dashboards | REUSAR | Fuentes de verdad |
| AdvancedReports God object | NO expandir | Solo widgets opcionales |

## Insight types
SalesDrop, FoodCostHigh, WasteSpike, CashVariance, SupplierCritical, LowStock, NegativeMargin, Opportunity

## Score dimensions 0–100
Financial · Operational · Procurement · FoodCost → EnterpriseScore weighted
