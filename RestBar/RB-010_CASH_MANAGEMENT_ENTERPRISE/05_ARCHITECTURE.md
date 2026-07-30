# 05 — ARCHITECTURE

---

# Capas (extiende Foundation — no rompe MVC)

```
Controllers/
  CashRegisterController.cs    (MVC + API)
  CashSessionController.cs
  CashMovementController.cs
  CashReportController.cs

Domain/Cash/                   (nuevo folder lógico)
  Entities/ (o Models/EnterpriseCash.cs)
  Services/
    CashSessionService.cs
    CashMovementService.cs
    CashReconciliationService.cs
    CashApprovalService.cs
    CashReportService.cs
    CashIntegrityService.cs
  Events/
  DTOs/

Infrastructure/Cash/
  CashPaymentHook.cs           (IPaymentPostProcessor)
  CashHashChainBuilder.cs

Interfaces/
  ICashSessionService.cs
  ...

Hubs/OrderHub.cs               (extend groups: cash_register_{id}, branch_{id}_cash)
```

**Regla:** `PaymentService` / `PaymentController` **no** contienen lógica de arqueo. Solo llaman `ICashPaymentHook.OnPaymentCompletedAsync`.

---

# Flujo de integración Payment (crítico)

```
PaymentController.CreatePayment
  → PaymentService (existente, certificado)
  → ICashPaymentHook (nuevo)
       IF method in {Efectivo, Cash, ...}
         REQUIRE open CashSession for user/register
         SET payment.CashSessionId
         INSERT CashMovement(Sale, In, amount)
       IF card/yappy/ach
         INSERT CashMovement(SaleNonCash, In, amount) — no affects expected cash
  → OrderHub NotifyPayment + NotifyCashMovement
```

Idempotency: reutilizar `Payment.IdempotencyKey` → `CashMovement.IdempotencyKey` UNIQUE.

---

# Cálculo Expected Cash (single source)

```
ExpectedCash = OpeningFloat
  + SUM(movements WHERE affects_cash_drawer AND direction=In)
  - SUM(movements WHERE affects_cash_drawer AND direction=Out)

affects_cash_drawer = true para:
  OpeningFloat, Sale(cash), PaidIn, Refund(cash out), PaidOut, Adjustment(in/out cash)

false para:
  SaleCard, SaleYappy, SaleACH, TipAccrual (reported separately)
```

Servicio: `CashReconciliationService.GetExpectedCashAsync(sessionId)`.

---

# CQRS light

| Write | Read |
|-------|------|
| CashSessionService | CashReportService |
| CashMovementService | CashQueryService (AsNoTracking) |
| Commands transaccionales | Z-report snapshot read-only |

---

# Background jobs (nuevo — primer hosted service productivo)

| Job | Frecuencia | Función |
|-----|------------|---------|
| `CashSessionStaleAlertJob` | 15 min | Sesión abierta > N horas → Notification |
| `CashDailyIntegrityJob` | 03:00 | Verificar hash chain + orphan payments |
| `CashAutoSuspendJob` | 01:00 | Business day rollover policy |

Registrar en `Program.cs` post-diseño aprobado.

---

# Feature flag

`FeatureFlags.EnableCashModule` — default false hasta UAT.

Branch override: `SystemSettings.RequireCashSessionForCashPayments`.

---

# Performance targets

| Operación | P95 |
|-----------|-----|
| Open session | 300ms |
| Record movement | 150ms |
| Payment hook | +50ms max |
| Close + Z snapshot | 2s |
| Dashboard snapshot | 500ms (cached 30s) |

1000 restaurants: ~5000 sessions/day, ~200k movements/day — PostgreSQL OK con índices.

---

# No romper certificación

- `IOrderService`, KDS, cancel flow: **sin cambios**  
- Payment API contracts: additive fields only (`cashSessionId` optional response)  
- Tests ORDER 119/119 en regression gate  
