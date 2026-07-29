# 08 — ARCHITECTURE REFACTOR PLAN

**Principio:** Refactors estrangler + facade. Cero big-bang.

---

# Fase F0.5 (esta) — Prep only

1. `Infrastructure/Foundation/TenantScope.cs`  
2. `Infrastructure/Foundation/FeatureFlags.cs`  
3. Policies enterprise en Program.cs  
4. `Extensions/` para DI futuro  
5. Test project smoke  
6. Harden secrets + seed + menús  
7. Índices DB  
8. Documentar extracción Order  

**No mover** Controllers/Views.

---

# Fase F0.6 — Order extraction (antes de Cash)

```
IOrderService (facade — misma interfaz)
  ├── OrderLifecycleService
  ├── OrderItemCommandService
  ├── KitchenQueryService  (merge KitchenService overlap)
  └── OrderPricingService  (discount + price schedule)
```

Controllers siguen inyectando `IOrderService`.  
Certificación ORDER 119/119 = gate.

---

# Fase F1+ — New modules as vertical slices

```
/Domain/Cash/{Entities, Services, ICashService}
/Domain/Purchasing/...
/Controllers/CashController.cs   // thin
```

DI: `services.AddCashModule()`.

---

# Patrones por módulo

| Módulo | Patrón |
|--------|--------|
| Cash | Session aggregate + ledger movements |
| Purchasing | Document flow Draft→Sent→Received→Closed |
| Costing | Read model from Recipe+Movements |
| Fiscal | Strategy/Adapter per country |
| BI | Separate schema + jobs |
| Copilot | Tools over read models |

---

# Reglas de merge PR arquitectura

- [ ] ¿Toca OrderService? Justificar o rechazar  
- [ ] ¿Filtro CompanyId/BranchId?  
- [ ] ¿Test tenant?  
- [ ] ¿Feature flag si incompleto?  
- [ ] ¿No rompe SignalR groups?
