# 06 — SECURITY MODEL

---

# Principios

1. **Segregación de funciones** — quien cobra ≠ quien aprueba varianza ≠ quien configura registers  
2. **Fail closed** — sin sesión abierta, no cash payment (cuando policy activa)  
3. **Inmutabilidad** — movimientos no editables; solo reversal autorizado  
4. **Tenant isolation** — TenantScope en 100% operaciones  
5. **Least privilege** — permisos granulares cash  

---

# Threat model

| Amenaza | Mitigación |
|---------|------------|
| Cajero oculta faltante | Blind close + supervisor approval |
| Manager amigo aprueba todo | Dual approval + audit ranking overrides |
| Cross-tenant IDOR | TenantScope + tests |
| Replay payment | IdempotencyKey payment + movement |
| Edición DB directa | Hash chain + integrity job |
| Sesión abierta días | Stale alert + auto-suspend configurable |
| Void sin autorización | Payment void requiere supervisor si > threshold |
| Fraude propinas | TipAllocation vs cash tips report |

---

# Autenticación

Cookie auth existente + claims: UserId, CompanyId, BranchId, UserRole.

Cash APIs additionally validate:
- Active user `IsActive`
- Branch `IsActive` (TenantSubscriptionMiddleware)
- Register belongs to user's branch

---

# Autorización — capas

1. **Policy** ASP.NET (`CashAccess`, `ManagerOrAbove`)  
2. **Permission** fine-grained strings (extend AuthService)  
3. **Business rule** (varianza, reopen, paid-out amount)  

---

# Dual approval matrix

| Acción | Cajero | Supervisor | Manager | Admin |
|--------|--------|------------|---------|-------|
| Abrir sesión | ✅ | ✅ | ✅ | ✅ |
| Cobrar (POS) | ✅ | ✅ | ✅ | ✅ |
| Paid-out < $20 | ✅ | ✅ | ✅ | ✅ |
| Paid-out ≥ $20 | ❌ | ✅ | ✅ | ✅ |
| Paid-out ≥ $100 | ❌ | ❌ | ✅ | ✅ |
| Cerrar sesión | ✅ | ✅ | ✅ | ✅ |
| Aprobar varianza > umbral | ❌ | ✅ | ✅ | ✅ |
| Reabrir sesión | ❌ | ❌ | ✅ | ✅ |
| Configurar register | ❌ | ❌ | ✅ | ✅ |
| Ver auditoría forense | ❌ | ⚠️ | ✅ | ✅ |
| Export Z holding | ❌ | ❌ | ⚠️ | ✅ |

Umbrales configurables por Company/Branch.

---

# Override pattern

```csharp
SupervisorOverride {
  Action, Reason (required min 10 chars),
  OriginalDecision, OverriddenBy, ApprovedAt
}
```

Siempre persiste en `CashAuditEvent` + `CashApproval`.

---

# Session security

- Timeout inactividad cajero: warning 30 min, suspend 8h (config)  
- Un register = máximo 1 sesión `Open|Operating` simultánea  
- Reopen requiere manager + incident record  

---

# SignalR

Join group `cash_register_{registerId}` solo si user.BranchId match register.BranchId.

---

# Secrets & PII

- No almacenar PAN tarjeta (Payment.Method string only)  
- IP/UserAgent en CashMovement/AuditEvent  
- Export Z sin datos personales clientes salvo requerido fiscal  
