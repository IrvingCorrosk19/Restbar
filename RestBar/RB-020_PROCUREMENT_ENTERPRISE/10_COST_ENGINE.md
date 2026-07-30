# 10 — COST ENGINE

---

# Objetivo

Cada recepción actualiza automáticamente el costo que alimenta:

Food Cost · Recipe theoretical · Inventory valuation · BI · Copilot

---

# Fórmulas

## Last Purchase Cost
```
LastPurchaseCost = unit_cost del receipt line
LastPurchaseAt = received_at
```

## Moving Average Cost (WAC)
```
AverageCost = (StockBefore * AverageCostBefore + QtyAccepted * UnitCost)
              / (StockBefore + QtyAccepted)

si StockBefore + QtyAccepted == 0 → AverageCost = UnitCost
```

## Product.Cost (política v1)
```
Product.Cost = AverageCost   // configurable futuro: Last | Average | Standard
```

---

# Cascada post-receipt

1. Update Product costs  
2. Append PriceHistory  
3. Invalidate recipe theoretical cost cache (compute on read v1)  
4. Emit SignalR `ProductCostChanged`  
5. Audit event `CostUpdated`  

---

# Theoretical Food Cost (RB-023)

```
TheoreticalCost(product) =
  SUM( RecipeLine.Quantity × Ingredient.AverageCost|Cost )
```

Food Cost % (RB-024):
```
FoodCost% = TheoreticalCost / SellingPrice
ActualFoodCost% = COGS period / Sales period  (BI fase)
```

---

# Qué NO hace el Cost Engine

- No modifica Price de venta  
- No escribe en Invoice (sales)  
- No crea InventoryMovement (eso es Receipt hook)  

---

# Performance

Batch por receipt: 1 SELECT products FOR UPDATE / reload, 1 SaveChanges.
