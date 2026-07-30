# 04 — ARCHITECTURE

```
Controllers/CopilotController
Services/Copilot/
  CopilotOrchestratorService
  CopilotContextResolver
  CopilotMemoryService
  CopilotIntentClassifier
  CopilotRecommendationService
  CopilotDecisionService
  CopilotActionService
  CopilotAuditService
  Tools/* (ExecutiveTool, SalesTool, FoodCostTool, ProcurementTool, CashTool)
Infrastructure/Copilot/
  DeterministicAiProvider
  IAiProvider
Domain/Copilot/CopilotIntent, Guardrails
Models/EnterpriseCopilot.cs
```

## Pipeline
```
User message
 → ContextResolver (tenant+role)
 → Guardrails (injection/length)
 → IntentClassifier
 → ToolRegistry.Invoke(allowed by RBAC)
 → Explanation + Recommendation + Decision ranking
 → Memory append
 → Audit
 → Response DTO (markdown + action cards)
```
