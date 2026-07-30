# 03 — DOMAIN MODEL

**Bounded context:** `CashControl`  
**Principio:** Payment = verdad de cobro; Cash = verdad de efectivo en drawer.

---

# Aggregates

## 1. CashRegister (Entity — configuración)

Recurso persistente por sucursal. No es una sesión.

```
CashRegister
  Id, CompanyId*, BranchId*
  Code, Name
  RegisterType: Physical | Virtual | Mobile | Station | Shared | Central
  DefaultOpeningFloat
  IsActive
  StationId? (caja por estación cocina/bar)
  RequiresBlindClose
  VarianceThresholdAmount / Percent
  AllowedPaymentMethods[] (config display)
  CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
```

## 2. CashSession (Aggregate Root — operación)

Un ciclo apertura→cierre en un register.

```
CashSession
  Id, CompanyId*, BranchId*
  CashRegisterId*
  ShiftId? (link laboral existente — NO duplicar Shift)
  SessionNumber (secuencial por register/día)
  Status: enum (ver doc 13)
  OpenedAt, ClosedAt?
  OpenedByUserId*, ClosedByUserId?
  SupervisorUserId?, ManagerUserId? (witness/approval)
  OpeningFloatDeclared
  OpeningFloatApproved
  -- Totals (calculated + snapshotted at close)
  ExpectedCash, CountedCash, Variance
  ExpectedCard, ExpectedDigital (Yappy/ACH)
  TotalSales, TotalRefunds, TotalTips, TotalPaidIn, TotalPaidOut
  BlindCloseEnabled
  CloseNotes
  RowVersion (concurrency)
  IsReopened, ReopenedFromSessionId?
```

## 3. CashMovement (Entity — ledger inmutable)

Cada hecho económico que afecta el drawer o la sesión.

```
CashMovement
  Id, CompanyId*, BranchId*
  CashSessionId*
  MovementType: enum (ver doc 11)
  Direction: In | Out
  Amount (decimal 18,2) — siempre positivo; direction indica signo
  CurrencyCode (default branch currency)
  PaymentId?, OrderId?, PaymentRefundId?
  RelatedMovementId? (reversal link)
  ReasonCode, Comments
  PerformedByUserId*, AuthorizedByUserId?
  IdempotencyKey? (pagos automáticos)
  SequenceNumber (monotonic per session)
  PreviousHash, RecordHash (cadena forense)
  CreatedAtUtc
  Source: Manual | Payment | Refund | Void | System | Adjustment
  DeviceId?, IpAddress?, UserAgent?
```

**Regla:** Nunca UPDATE Amount. Corrección = movimiento inverso + aprobación.

## 4. CashCount (Entity — arqueo físico)

```
CashCount
  Id, CashSessionId*
  CountType: Opening | MidShift | Closing | SpotCheck
  CountedAtUtc, CountedByUserId*
  WitnessUserId?
  Denominations[] (CashDenominationLine)
  TotalCounted
  IsBlind (cajero no vio expected antes)
```

```
CashDenominationLine
  DenominationValue (0.01, 0.05, 1, 5, 20...)
  Quantity
  Subtotal
```

## 5. CashApproval (Entity — dual control)

```
CashApproval
  Id, CashSessionId*, CashMovementId?
  ApprovalType: Variance | Reopen | LargePaidOut | RefundOverride | SessionClose
  RequestedBy*, ApprovedBy*, Status: Pending|Approved|Rejected
  ThresholdAmount?, ActualAmount?
  Reason, CreatedAt, ResolvedAt
```

## 6. CashIncident (Entity — eventos operativos)

```
CashIncident
  Id, CashSessionId*
  IncidentType: Shortage | Overage | SuspiciousVoid | ForcedClose | SystemError
  Severity: Low | Medium | High | Critical
  Description, ResolvedBy?, ResolvedAt?, ResolutionNotes
```

## 7. CashAuditEvent (Entity — forense dedicada)

Complementa `AuditLog` global con campos cash-specific y hash chain.

```
CashAuditEvent
  Id, CompanyId*, BranchId*, CashSessionId?, CashMovementId?
  EventType, ActorUserId*, ActorRole
  BeforeJson, AfterJson
  IpAddress, DeviceId
  PreviousEventHash, EventHash
  CreatedAtUtc
```

---

# Value Objects

- `Money` (Amount + Currency) — usar decimal + Currency from branch  
- `VarianceResult` (Expected, Counted, Delta, Percent)  
- `ZReportSnapshot` (immutable DTO at close)  

---

# Domain Services (interfaces)

```
ICashSessionService      — open, suspend, close, reopen
ICashMovementService     — record manual, auto from payment
ICashReconciliationService — expected cash calculator
ICashApprovalService     — dual control workflow
ICashReportService       — Z/X reports
ICashIntegrityService    — hash chain verify
```

---

# Extensiones a entidades existentes (NO duplicar)

| Entidad | Campo nuevo |
|---------|-------------|
| `Payment` | `CashSessionId?` (nullable hasta migración) |
| `PaymentRefund` | `CashSessionId?` |
| `Shift` | Navigation `ICollection<CashSession>` (optional FK inverse) |

**NO modificar** semántica certificada de Payment amount/method/idempotency.

---

# Eventos de dominio (in-process v1)

```
CashSessionOpened
CashMovementRecorded
CashSessionClosed
CashVarianceDetected
CashApprovalRequired
CashIncidentRaised
```

Publicar vía `ICashEventPublisher` → OrderHub + NotificationService.
