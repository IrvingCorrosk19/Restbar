# 03 — DOMAIN MODEL

| Entidad | Acción |
|----------|--------|
| CopilotConversation | CREAR |
| CopilotMessage | CREAR |
| CopilotMemoryItem | CREAR |
| CopilotAuditEvent | CREAR |
| CopilotActionLog | CREAR |
| IAiProvider | CREAR interfaz |
| DeterministicAiProvider | CREAR v1 |
| ToolRegistry + ICopilotTool | CREAR |
| Executive CC / FC / PO / Cash services | REUSAR |

## Intents v1
ExecutiveBriefing, SalesToday, FoodCostWhy, PurchasingWhat, CashStatus, AlertsNow, WhatShouldIDo, RecommendMenu, WasteStatus, SupplierAdvice, Help, Unknown
