# 10 — CASH REGISTER

---

# Tipos (RegisterType)

| Tipo | Uso LATAM | v1 |
|------|-----------|-----|
| **Physical** | Caja física mostrador | ✅ |
| **Virtual** | Caja lógica sin drawer (solo tracking) | ✅ |
| **Mobile** | Tablet mesero cobra en mesa | v1.1 |
| **Station** | Bar/cocina quick cash | v1.1 |
| **Shared** | Un drawer varios cajeros (turnos) | ✅ |
| **Central** | Única caja sucursal pequeña | ✅ |
| **Temporary** | Evento/pop-up | v2 |
| **SelfService** | Kiosk | v2 |
| **Delivery** | Rider cobra | v2 |
| **Franchise** | Register bajo reglas marca | v2 |

v1 implementa Physical, Virtual, Shared, Central.

---

# Configuración por register

```yaml
Code: "CAJA-01"
Name: "Caja Principal"
BranchId: required
DefaultOpeningFloat: 200.00
RequiresBlindClose: true
VarianceThresholdAmount: 5.00
VarianceThresholdPercent: 0.1
MaxPaidOutWithoutApproval: 20.00
AllowedRoles: [cashier, supervisor, manager]
StationId: null  # optional bar register
AutoAssignToUser: false  # vs explicit login to register
BusinessDayCutoffHour: 4  # 4 AM = new business day
```

---

# Reglas

- UNIQUE(BranchId, Code)  
- Cannot delete register with historical sessions (deactivate only)  
- Cannot open 2 sessions Open on same register  
- Register inactive → no new sessions  

---

# Franquicia / Holding (diseño)

Company-level template registers cloned to branches (Fase franchise pack). v1: manual CRUD per branch.

---

# UX

Manager → Settings → Cash Registers → list/grid → create/edit wizard.

Cajero → Opening Wizard picks from assigned registers only (`UserAssignment` extend optional `AssignedRegisterIds`).
