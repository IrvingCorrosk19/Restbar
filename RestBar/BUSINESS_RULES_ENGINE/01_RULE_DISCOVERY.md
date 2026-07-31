# 01 — Rule Discovery

**Estado:** PASS · **Fecha:** 2026-07-31

## Engines existentes (no BRE general)

| Componente | Path | Nota |
|------------|------|------|
| CashSessionStateMachine | Domain/Cash | KEEP_IN_CODE |
| PurchaseOrder/Request SM | Domain/Procurement | KEEP_IN_CODE |
| BiInsight/Alert/Score | Services/Intelligence | MIGRATABLE thresholds |
| FoodCost + MenuEng | Domain/FoodCost | thresholds MIGRATABLE |
| Forecast + DI rules | Domain/DecisionIntelligence | MIGRATABLE |
| Hash chains | Infrastructure/* | NOT_CONFIGURABLE |

## Inventario clasificado (extracto crítico)

| ID | Regla | Clase |
|----|-------|-------|
| T-01 | Stock ≤ MinStock | CONFIGURABLE (ya en producto) |
| T-04/T-07 | Cash variance threshold (5m DI / register) | MIGRATABLE → plantilla |
| T-09 | Food Cost ≥ 35% o var ≥ 2 pts | MIGRATABLE |
| T-10 | Waste ≥ 50 | MIGRATABLE |
| T-11 | Sales drop ≥ 20% | MIGRATABLE |
| T-13 | PO dual approval ≥ 500 | MIGRATABLE |
| T-17 | Reorder lead+safety days | MIGRATABLE |
| V-01..V-15 | Invariantes orden/caja/stock/PO | KEEP_IN_CODE |
| A-04..A-06 | Hash chains | NOT_CONFIGURABLE |
| I-01..I-05 | Insights/alerts/DI recs | MIGRATABLE vía templates |
| FeatureFlags | Module gates | CONFIGURABLE |

## Política de migración

- **v1:** plantillas publicables que generan Alert / Recommendation / Notification / Audit — **sin** reemplazar state machines.
- BiInsightEngine permanece como fallback hasta que reglas publicadas cubran los mismos códigos.
- Duplicar umbrales (DI 5m vs CashRegister) se unifica documentando variables de regla `cash.varianceAbs` vs registro.

## Detalle completo

Ver evidencia en discovery agent RB-029 (thresholds T-01…T-23, validations V-01…V-16, autos A-01…A-12, insights I-01…I-08, approvals P-01…P-05).
