# 06–12 — ENGINES (resumen)

## Cost Engine
Política Company: Average (default) | Last | Standard (Product.Cost manual freeze).  
FIFO preparado (PriceHistory ordered) — no default v1.

## Recipe Engine
Theo cost, FoodCost%, Margin, Contribution, Yield, Waste%. History on recipe save / cost change.

## Profitability
GrossProfit, GrossMargin%, Contribution, FoodCost% Theo/Actual, COGS period. Labor/Prime v1.1.

## Variance
Actual − Theo; alert if |pts| > threshold (default 2.0). Types: OverUsage, WasteSpike, CostSpike.

## Waste
WasteEvent + movement UnitCost = AverageCost. Impact → Actual COGS + dashboard.

## Menu Engineering
BCG: Star (hi pop, hi marg), PlowHorse (hi pop, lo marg), Puzzle (lo pop, hi marg), Dog (lo, lo).  
Actions: promote / reprice / reformulate / remove.

## Price Simulation
In-memory: Δprice, Δingredient cost, Δrecipe qty → new margin/FC% before apply.
