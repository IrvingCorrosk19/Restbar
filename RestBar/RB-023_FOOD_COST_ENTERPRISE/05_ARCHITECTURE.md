# 05 — ARCHITECTURE

```
Domain/FoodCost/          FoodCostMath, MenuEngineeringClassifier
Models/EnterpriseFoodCost.cs + RestBarContext.FoodCost.cs
Services/FoodCost/
  FoodCostEngine.cs          theoretical/actual/variance/period
  RecipeProfitabilityService.cs
  WasteService.cs
  MenuEngineeringService.cs
  CostSimulationService.cs
  FoodCostDashboardService.cs
  FoodCostIntegrityService.cs
Infrastructure/FoodCost/
  OrderItemCostHook.cs       IOrderItemCostHook
  FoodCostHashChainBuilder.cs
Extensions/EnterpriseFoodCostExtensions.cs
Controllers/
  FoodCostDashboardController
  RecipeCostController (UI)
  WasteController (enhance)
```

## Hook venta
```
OrderService.AddItem → SaveChanges → IOrderItemCostHook.ApplyAsync(item)
  IF EnableFoodCostModule:
    item.TheoreticalUnitCost = FoodCostEngine.GetPlateCost(productId)
    item.CostSnapshotAt = UtcNow
```

## No romper
OrderService: solo inyección opcional + 3 call sites post-add.  
InventoryMovementController.CreateWaste → WasteService.
