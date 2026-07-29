# 04 — COBERTURA DE KPIs

**Leyenda:** ✅ Medible hoy · ⚠️ Parcial · ❌ No disponible

---

## Ventas e ingresos

| KPI | Estado | Fuente verificada |
|-----|--------|-------------------|
| Ventas diarias | ✅ | `GetDailySales`, PaymentView |
| Ventas mensuales | ✅ | DashboardStats |
| Ticket promedio | ✅ | GetSalesMetrics |
| Ticket por mesero | ⚠️ | GetEmployeeSales (no tiempo real) |
| Ticket por sucursal | ✅ | GetBranchSales |
| Ticket por empresa | ⚠️ | Multitenant + agregación manual |
| Ventas por categoría | ✅ | GetCategorySales |
| Ventas por producto | ✅ | GetTopProducts |

---

## Operación

| KPI | Estado | Fuente |
|-----|--------|--------|
| Tiempo preparación cocina | ⚠️ | StationPerformance (histórico, no SLA alert) |
| Tiempo entrega | ❌ | No medido end-to-end |
| Ocupación mesa | ⚠️ | GetTableUtilization |
| Rotación mesas | ⚠️ | Derivado, no KPI dedicado |
| % cancelaciones | ⚠️ | audit_logs + orders; sin widget |
| Descuentos aplicados | ✅ | GetDiscounts |
| Cortesías | ❌ | Sin tipo dedicado |
| Reembolsos | ⚠️ | payment_refunds; reporte limitado |

---

## Personal

| KPI | Estado | Fuente |
|-----|--------|--------|
| Productividad mesero | ⚠️ | Employee sales |
| Productividad cocina | ⚠️ | Station performance |
| Propinas | ⚠️ | Modelo TipAmount; reporte débil |
| Comisiones | ⚠️ | CommissionRule + cálculo en reporte empleado |

---

## Inventario y compras

| KPI | Estado | Fuente |
|-----|--------|--------|
| Exactitud inventario | ⚠️ | Stock por producto/estación |
| Productos bajo mínimo | ✅ | InventoryAnalysis |
| Food cost % | ❌ | Sin costo compra integrado |
| Beverage cost % | ❌ | Idem |
| Cumplimiento compras | ❌ | Sin PO |

---

## Rentabilidad

| KPI | Estado | Fuente |
|-----|--------|--------|
| Margen bruto producto | ✅ | GetProductProfitability |
| Margen por categoría | ✅ | GetCategoryProfitability |
| Margen neto | ❌ | Sin costos operativos/labor |

---

## Resumen cobertura

| Categoría KPI | Cobertura |
|---------------|-----------|
| Ventas básicas | **75%** |
| Operación tiempo real | **35%** |
| Finanzas / costos | **25%** |
| Compras | **0%** |
| **Total ponderado** | **~45%** |
