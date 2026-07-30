# 02 — REQUIREMENTS

## MUST
| ID | Req |
|----|-----|
| FR-01 | Theoretical cost por receta/plato (reusar + enriquecer) |
| FR-02 | Snapshot TheoreticalUnitCost en OrderItem al vender (flag ON) |
| FR-03 | Actual food cost período (usage: Sale+Waste valued) |
| FR-04 | Variance Actual vs Theoretical + alertas |
| FR-05 | WasteEvent estructurado + UnitCost en movimiento |
| FR-06 | Menu engineering Stars/Plowhorses/Puzzles/Dogs |
| FR-07 | Gross margin / contribution / food cost % |
| FR-08 | Price/cost simulation (what-if, no persist obligatorio) |
| FR-09 | Executive Food Cost Command Center <5s |
| FR-10 | Feature flag EnableFoodCostModule default false |
| FR-11 | Recipe UI mínima (cost + margen) |
| FR-12 | CostingAccess en superficie food cost |
| FR-13 | Audit hash chain food cost events |

## SHOULD v1.1
FIFO ledger · Labor/energy cost · Foto waste · Franchise rollup · Background nightly snapshot job

## NFR
0 regresiones Cash/Procurement/POS · AsNoTracking dashboards · índices período
