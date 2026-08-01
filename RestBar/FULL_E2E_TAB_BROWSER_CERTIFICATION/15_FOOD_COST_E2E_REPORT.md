# 15 — FOOD COST E2E REPORT (Tab Browser)

**Dominio:** FoodCost dashboard, recipes, menu engineering, plate cost  
**Pack fecha:** 2026-08-01  
**Build:** Release deploy `871abc7`

| ID | Escenario | Estado | Notas |
|----|-----------|--------|-------|
| E2E-FC-01 | FoodCost + recipes pages | NOT STARTED (este pack) | Referencia: prior `foodcost` / RB evidence — no re-run aquí |
| E2E-FC-02 | Plate cost after order/recipe | NOT STARTED | Cadena POS→FC no ejecutada |
| E2E-FC-03 | Menu engineering smoke | NOT STARTED | — |
| Flag EnableFoodCostModule | Production | Control known | Module gated |
| Unit FoodCost math | Unit tests | Referencia | ~98 unit pass histórico |
| Prior suite foodcost | chromium-desktop | Referencia previa | 161 PASS baseline — global re-run IN PROGRESS |

## Gaps vs mandato

- Deep food-cost chain ligada a pedido real: NOT STARTED  
- Recipe cross-tenant isolation deep: pendiente

**Veredicto dominio Food Cost:** FAIL vs mandato deep E2E (NOT STARTED en este pack).
