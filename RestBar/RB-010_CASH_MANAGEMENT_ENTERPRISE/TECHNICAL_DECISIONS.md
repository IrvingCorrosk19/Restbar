# RB-010 — TECHNICAL DECISIONS

**Fecha:** 2026-07-29

---

1. **Feature flag default off** — `EnableCashModule=false` hasta UAT; evita romper POS en producción.
2. **Partial RestBarContext** — `RestBarContext.Cash.cs` para configuración EF sin inflar archivo principal.
3. **Hook pattern** — `ICashPaymentHook` invocado desde PaymentController/PaymentService; PaymentService core sin lógica de arqueo.
4. **Expected cash denormalized** — `CashSession.ExpectedCash` actualizado en cada movimiento para dashboard rápido; recalculable desde ledger.
5. **Hash chain in-process** — SHA-256 sobre payload canónico; verificación via `VerifyMovementChainAsync`.
6. **Shift optional** — `CashSession.ShiftId` nullable; extiende Shift sin reemplazarlo.
7. **Dual approval async** — Paid-out grande retorna 202 + approval record; no bloquea API con polling.
8. **Migration additive** — FK nullable en payments/payment_refunds; cero impacto datos históricos.
