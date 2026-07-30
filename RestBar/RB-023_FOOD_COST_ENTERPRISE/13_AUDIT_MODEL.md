# 13 — AUDIT MODEL
FoodCostAuditEvent: hash chain SHA-256. Events: RecipeCostRecalc, WasteRecorded, SnapshotGenerated, VarianceAlert, SimulationRun, CostPolicyChanged.

# 14 — SECURITY
CostingAccess (admin, manager, accountant, chef). Waste approve supervisor+. Flag EnableFoodCostModule. MT Company/Branch.

# 15 — EXECUTIVE COMMAND CENTER
Widgets: FC% hoy, margen, Stars/Dogs, waste $, variance, top margin killers, branch compare. <5s AsNoTracking.

# 16 — REPORTS
Recipe Cost · Period AvT · Menu Engineering · Waste Log · Category FC% · Simulation export v1.1

# 17 — KPIs
Theo FC% · Actual FC% · Variance pts/$ · Waste% · Gross Margin · Contribution · Avg Recipe Cost · Dead inventory (stock*cost sin venta 30d)

# 18 — TEST PLAN
Unit: FoodCostMath, MenuEngineeringClassifier, WAC reuse. Integration: snapshot on sale (flag). Regression: Cash 25 + Proc + Foundation. Build 0 errors.

# 19 — IMPLEMENTATION PLAN
A Domain/EF · B Services · C Hooks Order+Waste · D Controllers/UI · E Dashboard/Reports · F Cert

# 20 — FINAL CERTIFICATION
Design gate ✅ · Post-impl: ver CERTIFICATION_RESULTS.md
