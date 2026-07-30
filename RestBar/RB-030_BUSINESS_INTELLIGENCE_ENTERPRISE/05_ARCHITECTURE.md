# 05 — ARCHITECTURE

```
Services/Intelligence/
  ExecutiveCommandCenterService  → Task.WhenAll(sources) + Insights + Alerts + Scores
  BiInsightEngine
  BiAlertEngine
  BiScoreEngine
  BiAuditService

Domain/Intelligence/BiDecisionMath.cs
Models/EnterpriseIntelligence.cs
RestBarContext.Intelligence.cs
Extensions/EnterpriseIntelligenceExtensions.cs
Controllers/ExecutiveCommandCenterController.cs
Views/ExecutiveCommandCenter/
```

## Flujo
```
GetExecutiveSnapshot(company, branch)
  → parallel: SalesMetrics, CashDash, ProcDash, FoodCostDash
  → InsightEngine.Generate(from composites)
  → AlertEngine.Evaluate
  → ScoreEngine.ComputeBranch
  → optional persist ExecutiveSnapshot
  → BiAudit
  → return DTO decisiones
```

Cache memoria 30s por BranchId.
