# 14 — SECURITY MODEL

---

# Policies (ya existentes — activar)

| Policy | Roles |
|--------|-------|
| PurchasingAccess | admin, manager, inventarista, accountant, supervisor |
| CostingAccess | admin, manager, accountant, chef |

Nuevas roles lógicas (claims existentes, sin enum nuevo v1): buyer = inventarista/supervisor.

---

# Tenant

Toda query filtrada por CompanyId del claim.  
BranchId en PR/PO/GR. Supplier es Company-scoped (compartido entre branches).

---

# Fraud controls

1. Dual approval montos altos  
2. Requester ≠ Approver  
3. Blacklist hard-stop  
4. Price override auditado  
5. Ad-hoc purchase alert cuando módulo ON  
6. Hash chain integridad  
7. RowVersion en PO  

---

# Feature flag rollback

`EnablePurchasingModule=false` → controllers ModuleDisabled; inventory ad-hoc sigue funcionando (comportamiento actual).
