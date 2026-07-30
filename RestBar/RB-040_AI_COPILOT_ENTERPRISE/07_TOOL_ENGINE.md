# 07 — TOOL ENGINE

| Tool | Reusa | Permiso |
|------|-------|---------|
| get_executive_snapshot | IExecutiveCommandCenterService | ReportAccess |
| get_sales_today | ISalesReportService / CC | ReportAccess |
| get_food_cost | IFoodCostEngine / CC | CostingAccess |
| get_procurement | IProcurementDashboardService | PurchasingAccess |
| get_cash_status | ICashReportService / session | CashAccess |
| get_alerts | IAlertEngineService | ReportAccess |
| get_recommendations | BiInsight | ReportAccess |
| draft_purchase_request | IPurchaseRequestService | PurchasingAccess |

Sin tool → no inventar números (guardrail).
