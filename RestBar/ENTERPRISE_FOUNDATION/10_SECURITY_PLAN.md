# 10 — SECURITY PLAN

---

# Gaps actuales

| Gap | Severidad | Mitigación |
|-----|-----------|------------|
| GetById sin tenant check (Order, Invoice, etc.) | Alta | TenantScope.EnsureAccess |
| Seed AllowAnonymous | Alta | Solo Development |
| Secrets en código OnConfiguring | Alta | Eliminar passwords |
| appsettings.Development passwords | Media | User secrets / env |
| SignalR join group por Guid | Media | Validar tenant en hub methods |
| Soft delete ausente | Baja-Media | IsActive discipline + audit |
| Staging = no Production | Alta | Tratar Staging como locked seed |

---

# RBAC evolution

Policies nuevas (F0.5):

- `CashAccess` — admin, manager, cashier, accountant  
- `PurchasingAccess` — admin, manager, inventarista, accountant  
- `CostingAccess` — admin, manager, accountant, chef  
- `FranchiseAccess` — admin, superadmin  

No cambian permisos actuales.

---

# Feature flags seguridad de producto

Ocultar superficies incompletas evita “seguridad por oscuridad” + mala UX:

- `EnableSupplierUi`  
- `EnableBackupExecution`  
- `EnableAdvancedSettingsExtra`  
- `EnableSeedEndpoints`  

---

# Checklist cada módulo nuevo

1. CompanyId/BranchId non-null en create  
2. Authorize policy dedicada  
3. Test IDOR cross-company  
4. AuditLog Module name  
5. No secrets in logs  
6. Seed never in prod/staging  

---

# IDOR test pattern (certificación)

```
Login Company A → Get resource Id of Company B → Expect 404/403
```

Incluir en Enterprise Tests suite.
