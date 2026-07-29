# 06 — DUPLICATION REPORT

---

# Duplicaciones confirmadas

| ID | Duplicado | Evidencia | Resolución foundation |
|----|-----------|-----------|------------------------|
| D01 | UserController vs UserManagementController | Dos UIs admin usuarios | Documentar consolidación; no borrar aún |
| D02 | `/Payment/Index` menú vs Payment API | PaymentController es ApiController — Index menú incorrecto | **FIX:** menú → `/PaymentView/Index` |
| D03 | Category vs ProductCategory | Product usa CategoryId; ProductCategory sin tenant | Deprecar ProductCategory (backlog) |
| D04 | Reports vs AdvancedReports | Overlap sales/top products | Unificar entry Command Center después |
| D05 | KitchenService vs OrderService KDS | Queries similares | Extraer KitchenQuery shared (plan) |
| D06 | Factory DI vs AddScoped genérico | Program.cs inconsistente | Normalizar gradual |
| D07 | Dual AddDbContext + AddScoped Context | Program.cs | Consolidar (cuidado tracking) |

---

# Huérfanos

| Artefacto | Estado | Acción |
|-----------|--------|--------|
| InvoiceService | Sin controller | Conservar para Fiscal |
| CustomerService | Solo Order | Conservar para CRM |
| ModifierService | Sin controller | Wire o marcar obsolete |
| NotificationService | Sin UI | Base alertas |
| ProductCategoryService | Legacy | Deprecar |
| supplier-management.js | 404 endpoints | Ocultar hasta Purchasing |
| SupplierAnalysis report | Stub ceros | Feature flag off |
| Backup ExecuteBackup | Delay fake | Feature flag / label |
| AdvancedSettings views faltantes | Links rotos | Ocultar links |
| GrowthForecasts vacío | UI engañosa | No mostrar hasta Forecast |

---

# Parcialmente implementado (extender, no reemplazar)

Shift API · Recipe API · PriceSchedule · DiscountPolicy · TipAllocation · Invoice model · LoyaltyPoints field · TenantSubscriptionMiddleware

---

# Renombres conceptuales (código opcional después)

| Actual | Nombre destino |
|--------|----------------|
| PriceScheduleService | IProductPricingService |
| CreatePurchase (InventoryMovement) | StockAdjustmentInbound (hasta PO real) |
| AdvancedReports | AnalyticsQueryService |
