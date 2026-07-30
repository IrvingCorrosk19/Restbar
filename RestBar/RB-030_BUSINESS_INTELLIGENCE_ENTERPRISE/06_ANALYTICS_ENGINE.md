# 06–12 ENGINES & DASHBOARDS

## 06 Analytics Engine
Orquestador read-only. Fuentes: ISalesReportService, ICashReportService, IProcurementDashboardService, IFoodCostDashboardService.

## 07 Insight Engine
Reglas → título + explicación + acción. Nunca solo el número.

## 08 Alert Engine
Umbrales: FC variance≥2pts, Waste>$50/día, Overdue PO>0, Critical suppliers, Sales drop >20% vs yesterday.

## 09 Score Engine
BranchScore = 0.3*Financial + 0.25*Ops + 0.25*FoodCost + 0.2*Procurement

## 10 Executive Command Center
Widgets decisión + Recommended Actions strip.

## 11 Operational Dashboards
Deep-links a Kitchen/Orders/Inventory (no duplicar UI).

## 12 Financial Dashboards
Deep-links Cash / FoodCost / Procurement.

# 13–20
Audit hash · Security EnableCommandCenter + ReportAccess/admin · Reporting wraps AdvancedReports real parts · KPIs orquestados · Tests math · Impl A–F · Tech decisions · Certification
