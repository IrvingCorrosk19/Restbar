# 05 — Report Catalog (Native HTML)

RestBar already ships native HTML/MVC reports. RB-025 adds a BI hub; it does **not** claim a full Power-BI clone.

## Existing native reports (evidence)

| Report | Route / Controller | Filters | Export | Charts | Auth |
|--------|--------------------|---------|--------|--------|------|
| Sales reports | `/Reports` · `ReportsController` | dates, metrics APIs | PDF/Excel **stubbed** unless `EnableReportExports` | via JS APIs | ReportAccess |
| Advanced reports | `/AdvancedReports` | profitability, ops, inventory, trends | Export actions present | yes | ReportAccess |
| Food Cost | `/FoodCostDashboard` | period, waste, plate cost | module UI | yes | CostingAccess + flag |
| Procurement | `/ProcurementDashboard` | supplier, theoretical cost | module UI | yes | PurchasingAccess + flag |
| Cash Z/X | `/CashReport/ZReport` | session | print | limited | CashAccess + flag |
| Executive CC | `/ExecutiveCommandCenter` | tenant claims | JSON snapshot | KPI cards | ReportAccess + flag |
| Payment analytics | `PaymentViewController.Analytics` | — | — | — | auth |
| **BI Nativo hub** | `/BiNative` | date range | CSV hourly + print | Chart.js hourly | ReportAccess |

## Feature completeness vs Phase 5 checklist

| Capability | Status on BiNative hub | Elsewhere |
|------------|------------------------|-----------|
| Filtros | dates | yes on most modules |
| Ordenamiento | client table | AdvancedReports |
| Búsqueda | product search | — |
| Export PDF | **NO en hub** | Reports stubs / EnableReportExports |
| Export Excel | **NO en hub** | stubs |
| Export CSV | hourly CSV | — |
| Impresión | `window.print` | Cash Z |
| Responsive | Bootstrap | yes |
| Tema oscuro | inherits layout | layout dark dropdowns |
| Gráficos interactivos | Chart.js hourly | Command Center limited |
| Drill-down | links to modules | Command Center actions |
| KPIs / tarjetas | yes | Command Center |
| Tablas dinámicas | basic | AdvancedReports |
| Comparativos / tendencias | BranchComparison API + Advanced trend | — |

## Immediate build candidates (data ready)

1. Sales by hour/day/product/waiter — **ready** (`BiNative` + Reports)
2. Cash variance pack — **ready** (CashReport + `sp_cash_summary`)
3. Food cost + waste — **ready** (FoodCost + `sp_top_waste`)
4. Supplier spend — **ready** (`sp_supplier_analysis` + Procurement)
5. Kitchen station prep — **ready** after SentAt/PreparedAt population

## Deferred (do not fake)

- Full Excel/PDF enterprise pack without `EnableReportExports` implementation
- Pivot “tabla dinámica” Excel-like engine
- Dark-theme design system overhaul
