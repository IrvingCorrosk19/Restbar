# 12 — MULTITENANT PLAN

---

# Modelo actual

```
Company (IsActive)
  └── Branch (IsActive)
        └── Users, Orders, Stations, Stock, …
Claims: CompanyId, BranchId, UserRole
Middleware: TenantSubscription blocks writes if inactive
```

Certificación: **51/51 PASS** — no romper.

---

# Gaps

| Gap | Plan |
|-----|------|
| Sin global query filters | Helper primero; filters EF opcionales F2+ |
| User solo BranchId | Resolver Company via Branch (ya común) |
| ProductCategory sin tenant | Deprecar |
| GetById id-only | TenantScope |
| Seed cross-tenant | Solo Dev |
| Nullable CompanyId en ops | Nuevos módulos: required |

---

# TenantScope (contrato)

```csharp
TenantContext {
  Guid? UserId, CompanyId, BranchId, Role
}
EnsureSameCompany(entity.CompanyId)
EnsureSameBranch(entity.BranchId) // cuando aplique
```

---

# Futuro Franchise

```
Franchisor (platform)
  └── Brand Company
        └── Franchisee Company (link)
              └── Branches
```

No modelar en F0.5 — dejar Company extensible (`ParentCompanyId` nullable futuro).

---

# SaaS billing tie-in

TenantSubscriptionMiddleware ya bloquea writes.  
Extender con `Subscriptions` table sin cambiar middleware contract (leer IsActive / plan limits).

---

# Reglas inviolables

1. Nunca confiar solo en ocultar IDs en UI  
2. Todo endpoint nuevo con test cross-tenant  
3. SuperAdmin es el único cross-company  
4. SignalR groups deben validar membership tenant  
