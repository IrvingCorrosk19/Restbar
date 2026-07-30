# 19 — IMPLEMENTATION PLAN

**Prerequisito:** Diseño RB-010 aprobado (este paquete) + F0.5 Foundation en main.

**Duración estimada:** 6–8 semanas (1–2 engineers)

---

# Fases implementación

## Phase A — Domain + DB (Sem 1–2)

| Task | Deliverable |
|------|-------------|
| A1 | Models `EnterpriseCash.cs` entities |
| A2 | EF migration M1 (tables + payment FK nullable) |
| A3 | Interfaces + stub services |
| A4 | Unit tests domain + state machine |
| A5 | Feature flag off |

## Phase B — Core services (Sem 2–3)

| Task | Deliverable |
|------|-------------|
| B1 | CashSessionService open/close |
| B2 | CashMovementService manual + hash chain |
| B3 | CashReconciliationService |
| B4 | CashApprovalService |
| B5 | Integration tests open/close |

## Phase C — Payment hook (Sem 3–4)

| Task | Deliverable |
|------|-------------|
| C1 | ICashPaymentHook + register DI |
| C2 | PaymentService integration (minimal touch) |
| C3 | Void/refund hooks |
| C4 | Regression ORDER + Payment tests green |

## Phase D — API + MVC UI (Sem 4–6)

| Task | Deliverable |
|------|-------------|
| D1 | Controllers API v1 |
| D2 | Opening Wizard + Dashboard + Close |
| D3 | Supervisor/Manager panels |
| D4 | POS badge + enforcement toggle |
| D5 | SignalR events |

## Phase E — Reports + CC (Sem 6–7)

| Task | Deliverable |
|------|-------------|
| E1 | Z-report PDF/Excel real |
| E2 | Command Center snapshot endpoints |
| E3 | Background stale alert job |

## Phase F — Certification (Sem 7–8)

| Task | Deliverable |
|------|-------------|
| F1 | Run full test plan doc 18 |
| F2 | Browser E2E 10 flows |
| F3 | MT 25+ cases |
| F4 | Fix defects until 100% PASS |
| F5 | Enable flag pilot branch |

---

# Orden estricto

```
DB → Services → Hook (sin UI) → API → UI → Reports → Jobs → Cert
```

**Prohibido:** UI antes de reconciliation correcta.

---

# Rollback

Feature flag `EnableCashModule=false` disables enforcement; tables remain; payments work without session (legacy mode).

---

# Definition of Done

- [ ] 0 compile errors  
- [ ] 0 test FAIL  
- [ ] ORDER 119/119 regression PASS  
- [ ] MT cash 25/25 PASS  
- [ ] Browser E2E close flow PASS  
- [ ] Z-report PDF generated  
- [ ] Design doc 20 sign-off updated  

---

# Team

- 1 backend lead (services + hook)  
- 1 full-stack (UI + API)  
- QA: cert scripts parallel week 6+  
