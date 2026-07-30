# 08 — PROFITABILITY ENGINE
GrossProfit = Sales − TheoCOGS · GrossMargin% · Contribution · ActualFC% · TheoFC%. Period Branch/Company. Ver `FoodCostEngine`.

# 09 — VARIANCE ENGINE
Variance$ / VariancePts. Alert si |pts| > 2. Types: OverUsage, WasteSpike, CostSpike.

# 10 — WASTE ENGINE
WasteEvent + InventoryMovement.Waste con UnitCost. ReasonCode enum.

# 11 — MENU ENGINEERING
Stars / PlowHorses / Puzzles / Dogs. Popularidad vs contribución mediana.

# 12 — PRICE SIMULATION
What-if en memoria: precio, costo ingrediente, qty receta → nuevo FC% y margen.

# 14 — SECURITY MODEL
CostingAccess + EnableFoodCostModule + MT + audit hash.

# 15 — EXECUTIVE COMMAND CENTER
Dashboard widgets FC/margen/waste/variance/menu. SignalR v1.1.

# 16 — REPORTS
AvT period, Recipe cost, Menu eng, Waste log.

# 17 — KPIS
Theo/Actual FC%, Variance, Waste%, Gross Margin, Contribution, Avg Recipe Cost.

# 18 — TEST PLAN
FoodCostMath + MenuEngineering + build/test suite.

# 19 — IMPLEMENTATION PLAN
A→F según arquitectura. Flag off hasta UAT.
