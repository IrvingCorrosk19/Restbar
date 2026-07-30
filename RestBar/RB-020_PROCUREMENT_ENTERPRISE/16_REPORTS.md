# 16 — REPORTS

| Reporte | Fuente | v1 |
|---------|--------|----|
| Supplier Analysis | Suppliers + Scores + Spend | ✅ reemplaza stub |
| Open PO Report | PurchaseOrders | ✅ |
| Receiving Log | GoodsReceipts | ✅ |
| Price Variance | PriceHistory | ✅ |
| Spend by Supplier | PO/Receipt totals | ✅ |
| Food Cost Theoretical | Recipe × Cost | ✅ |
| Purchase Audit | ProcurementAuditEvents | ✅ |
| Three-way match | + SupplierInvoice | v1.1 |
| Export PDF/Excel | EnableReportExports | v1.1 |

AdvancedReports.GetSupplierAnalysisAsync → delegar a ProcurementReportService cuando flag ON.
