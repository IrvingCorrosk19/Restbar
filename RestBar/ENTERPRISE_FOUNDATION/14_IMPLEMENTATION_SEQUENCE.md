# 14 — IMPLEMENTATION SEQUENCE

Orden estricto para no reescribir arquitectura.

```
F0.5 Foundation (AHORA)
  docs + helpers + policies + indexes + tests + harden
       │
       ▼
F0.6 Order extraction + TenantScope on mutations
       │
       ▼
F1 Cash (extiende Shift) + Precuenta (extiende Invoice)
       │
       ▼
F2 Purchasing (extiende InventoryMovement) + Recipe costing UI
       │
       ▼
F3 Promos/Combos (extiende DiscountPolicy) + Fiscal adapter
       │
       ▼
F4 Command Center + BI schema + jobs + SaaS billing
       │
       ▼
F5 Copilot + Franchise + Labor/Delivery
```

**Gate entre fases:** `dotnet build` + `dotnet test` + cert suite relevante en verde.

**Prohibido:** Empezar F1 Cash antes de cerrar FF-01…FF-11 y plan FF-20 documentado.
