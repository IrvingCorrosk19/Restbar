# 03 — Enterprise KPI Catalog
**Estado:** IMPLEMENTED (fuente unica: `Domain/Analytics/KpiCatalog.cs` + RB-025)
RB-028 **no duplica formulas**. Todo KPI ejecutivo debe citar codigo de `KpiCatalog`.
Ver tambien `RB-025_NATIVE_BI_ENTERPRISE/05_KPI_CATALOG.md`.
Nuevos KPIs DI (meta, no inventar datos):
| ID | Nombre | Disponibilidad |
|----|--------|----------------|
| DI.FCST_SALES_7D | Forecast ventas 7d | AvailableWithLimitations (ForecastEngine) |
| DI.FCST_MAPE | MAPE backtest | Available |
| DI.DQ_SCORE | Data quality score | Available (banner) |
| DI.LABOR_COST_PCT | Costo laboral / ventas | NotAvailable (sin nomina) |
| DI.GUESTS | Comensales | RequiresModelChange |
