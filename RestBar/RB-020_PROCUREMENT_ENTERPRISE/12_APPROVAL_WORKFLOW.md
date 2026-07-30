# 12 — APPROVAL WORKFLOW

---

# Matriz

| Acción | Cajero/Buyer | Supervisor | Manager | Admin |
|--------|--------------|------------|---------|-------|
| Crear PR/PO Draft | ✅ | ✅ | ✅ | ✅ |
| Submit PR | ✅ | ✅ | ✅ | ✅ |
| Approve PR < umbral | | ✅ | ✅ | ✅ |
| Approve PO ≥ umbral | | dual* | ✅ | ✅ |
| Blacklist supplier | | | ✅ | ✅ |
| Emergency PO (sin PR) | | ✅+audit | ✅ | ✅ |
| Short-close PO | | | ✅ | ✅ |
| Override precio >5% vs agreed | | | ✅ | ✅ |

\* Dual: requester ≠ approver.

---

# Umbrales default (Company configurable futuro)

| Tipo | Default |
|------|---------|
| PO auto-approve max | 200 |
| Dual approval from | 500 |
| Variance receipt % | 5% |
| Price override % | 5% |

---

# PurchaseApproval entity

Registra request/resolve para PR, PO, Variance, Emergency.  
Status Pending|Approved|Rejected. Audit hashed.
