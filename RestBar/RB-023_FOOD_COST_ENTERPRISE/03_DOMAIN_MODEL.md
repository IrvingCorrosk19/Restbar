# 03 — DOMAIN MODEL

## Decisiones
| Entidad | Acción | Por qué |
|---------|--------|---------|
| Recipe | EXTENDER | Yield%, TargetFoodCost%, Version |
| RecipeLine | EXTENDER | WastePercent |
| OrderItem | EXTENDER | TheoreticalUnitCost, CostSnapshotAt |
| InventoryMovement | REUSAR | set UnitCost en Waste |
| ProcurementCostEngine | REUSAR | WAC + theoretical base |
| **FoodCostSnapshot** | CREAR | Agregado período Company/Branch |
| **RecipeCostHistory** | CREAR | Histórico costo receta |
| **WasteEvent** | CREAR | Merma estructurada (RB-030) |
| **MenuEngineeringRow** | CREAR opcional / compute | BCG classification |
| **VarianceAlert** | CREAR | Eventos variance |
| **FoodCostAuditEvent** | CREAR | Hash chain |
| **CostSimulation** | NO persistir v1 | Cálculo en memoria |

## Fórmulas núcleo
```
TheoPlate = Σ(RecipeLine.Qty * (1+Waste%) * Ingredient.AverageCost|Cost) / YieldFactor
FoodCost% = TheoPlate / SellingPrice
GrossMargin = SellingPrice - TheoPlate
Contribution = (UnitPrice - Theo) * QtySold

TheoreticalPeriod = Σ(OrderItem.TheoCost * Qty)  [o recompute]
ActualPeriod = Σ(|Sale|+|Waste| movements * UnitCost|AvgCost)
Variance$ = Actual - Theoretical
VariancePts = Actual% - Theoretical%
```

## Menu engineering
- Popularidad: qty sold vs avg  
- Rentabilidad: contribution margin  
- Cuadrantes: Star / PlowHorse / Puzzle / Dog
