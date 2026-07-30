# RB-023 — TECHNICAL DECISIONS

1. **Reusar** `ProcurementCostEngine` WAC; FoodCostEngine calcula plate/period encima.  
2. **Snapshot** `OrderItem.TheoreticalUnitCost` vía hook flag-gated — no rompe POS.  
3. **Actual COGS** = movimientos Sale/Waste valuados + WasteEvents; fallback theo+waste.  
4. **Menu engineering** compute-on-read 30d (no tabla BCG persistida v1).  
5. **Simulation** en memoria.  
6. **WasteEvent** nuevo (RB-030) con UnitCost en InventoryMovement.  
7. Recipe UI bajo CostingAccess + EnableFoodCostModule.
