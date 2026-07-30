# 19 — IMPLEMENTATION PLAN

---

# Fases (orden obligatorio)

## Phase A — Domain
Entities, EF partial, migration, indexes, state machines, unit tests.  
Compile green.

## Phase B — Services
Supplier, PR, PO, Receipt, CostEngine, Score, Approval, Integrity, Hash.  
Integration-style unit tests. Compile green.

## Phase C — Inventory hook
GoodsReceipt → InventoryOps + Cost.  
NO romper Order/Cash. Regression tests.

## Phase D — Controllers + MVC/API
Supplier (JS-compatible), PR, PO, Receiving wizard, Dashboard, Approvals.  
Feature flag gated.

## Phase E — Reports + Command Center + KPIs
SupplierAnalysis real, spend reports, widgets.  
Wire AdvancedReports when flag ON.

## Phase F — Certification
Build, tests, docs: IMPLEMENTATION_PROGRESS, CERTIFICATION_RESULTS,  
PERFORMANCE/REGRESSION/SECURITY/MT/BUILD reports, TECHNICAL_DECISIONS.  
Update MASTER_BACKLOG, ROADMAP, CHANGELOG.

---

# Estimación

| Fase | Esfuerzo relativo |
|------|-------------------|
| A | 15% |
| B | 25% |
| C | 15% |
| D | 25% |
| E | 10% |
| F | 10% |

---

# Rollback

1. Flag OFF  
2. `dotnet ef migrations remove` / down script  
3. Ad-hoc inventory permanece
