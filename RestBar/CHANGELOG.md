# CHANGELOG — RestBar Enterprise

## [Unreleased] — RB-040 AI Copilot · Director Operativo Inteligente

### Added
- Orquestador Copilot (intent → tools → explicación → recomendaciones → acciones)
- Tools: executive snapshot, food cost, procurement, cash (reusan RB-010/020/023/030)
- `IAiProvider` + `DeterministicAiProvider` (sin vendor lock; listo para OpenAI/Azure/Claude)
- Persistencia: Conversation, Message, Memory, Audit, ActionLog
- Migration `AiCopilotEnterprise`
- UX `/Copilot` panel + suggested questions + action cards
- Design pack `RB-040_AI_COPILOT_ENTERPRISE/`
- Tests Copilot (suite **69/69 PASS**)

### Security
- `EnableCopilot` default **false**
- Guardrails prompt-injection · RBAC por tool · audit hash

---

## [Unreleased] — RB-030 Business Intelligence & Executive Command Center

### Added
- Executive Command Center orquestando Sales + Cash + Procurement + Food Cost
- Insight Engine / Alert Engine / Score Engine (Enterprise Score)
- Persistencia: ExecutiveSnapshot, BiAlert, BiScore, BiAuditEvent, ForecastSeed
- Migration `BusinessIntelligenceEnterprise`
- Design pack `RB-030_BUSINESS_INTELLIGENCE_ENTERPRISE/`
- Tests intelligence (+59 total)

### Security
- `EnableCommandCenter` default **false**
- Policy ReportAccess

---

## [Unreleased] — RB-023 Food Cost & Profitability Engine

### Added
- Motor Food Cost: teórico, actual, variance AvT, menu engineering (BCG), waste costed, simulation
- Snapshot `OrderItem.TheoreticalUnitCost` (flag-gated)
- Recipe Yield%/Waste%, Recipe Cost UI, Food Cost Command Center
- Migration `FoodCostEnterprise`
- Design pack `RB-023_FOOD_COST_ENTERPRISE/`
- Tests food cost (+54 total suite)

### Security
- `EnableFoodCostModule` default **false**
- Policy CostingAccess

---

## [Unreleased] — RB-020 Procurement Enterprise

### Added
- Plataforma de compras: Supplier, SupplierProduct, PurchaseRequest, PurchaseOrder, GoodsReceipt
- Cost Engine (WAC + LastCost) → Product.Cost / AverageCost / LastPurchaseCost
- Supplier Score (OTIF/Quality/Price/Reliability)
- Procurement Command Center + dual approval PO
- Controllers: Supplier, PurchaseOrder (wizard recepción), ProcurementDashboard
- Migration `ProcurementEnterprise`
- 20 unit tests procurement (+45 total suite)
- Design pack `RB-020_PROCUREMENT_ENTERPRISE/` (01–20)

### Security
- Feature flag `EnablePurchasingModule` default **false**
- Policies PurchasingAccess / CostingAccess enforced

---

## [Unreleased] — RB-010 Cash Management Enterprise

### Added
- Módulo RB-010: CashRegister, CashSession, CashMovement, CashCount, CashApproval, CashIncident, CashAuditEvent, CashZReport
- Servicios: Session, Movement, Reconciliation, Approval, Report, Integrity, Register
- `ICashPaymentHook` integrado en PaymentController y PaymentService (refunds)
- Controllers MVC/API: CashRegister, CashSession, CashMovement, CashReport
- Views: Dashboard, Opening Wizard, Detail, Arqueo, Reconciliation, Z Report
- SignalR: grupos `cash_register_{id}`, `cash_dashboard`
- Migration `CashManagementEnterprise` + índices operativos
- 15 unit tests cash (state machine + hash chain)
- Menú Operaciones → Caja / Cajas registradoras

### Changed
- `Payment.CashSessionId`, `PaymentRefund.CashSessionId` (nullable FK)
- `Program.cs`: `AddEnterpriseCashModule()`

### Security
- Policy `CashAccess` para superficie cash
- Feature flag `EnableCashModule` default **false**

### Documentation
- IMPLEMENTATION_PROGRESS, CERTIFICATION_RESULTS, BUILD/PERFORMANCE/REGRESSION/SECURITY/MT reports
- TECHNICAL_DECISIONS en RB-010_CASH_MANAGEMENT_ENTERPRISE/
