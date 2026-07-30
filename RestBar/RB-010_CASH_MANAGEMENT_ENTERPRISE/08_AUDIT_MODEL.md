# 08 — AUDIT MODEL

---

# Dos capas (complementarias)

| Capa | Uso |
|------|-----|
| `AuditLog` + `AuditMiddleware` | HTTP requests globales (existente) |
| `CashAuditEvent` | Forense cash con hash chain |

---

# CashAuditEvent — campos obligatorios

- EventType (enum: SessionOpened, MovementCreated, SessionClosed, ApprovalGranted, OverrideApplied, ReopenRequested, HashChainBroken, ...)  
- ActorUserId, ActorRole, CompanyId, BranchId  
- CashSessionId, CashMovementId (nullable)  
- BeforeJson, AfterJson (delta semántico)  
- IpAddress, DeviceId, UserAgent  
- PreviousEventHash, EventHash (SHA-256)  
- CreatedAtUtc  

---

# Hash chain (integridad)

```
RecordHash = SHA256(
  PreviousHash + SessionId + MovementId + Type + Amount + Direction + 
  PerformedBy + CreatedAtUtc + SequenceNumber
)
```

Primer evento sesión: `PreviousHash = "GENESIS-{SessionId}"`.

Job `CashDailyIntegrityJob` recalcula cadena; alerta si break.

---

# Qué auditar (lista exhaustiva)

Apertura, cierre, suspend, reopen, cada movement manual, auto payment hook, void, refund, approval grant/deny, count submit, incident create/resolve, config register change, threshold change, export Z, blind close toggle.

---

# Retención

- OLTP: 24 meses hot  
- Archive: JSON export mensual a storage (BackupSettings extend)  
- Legal min: 7 años diseño (jurisdicción configurable)  

---

# Auditor UX

`Auditor Panel`: timeline filtrable por session, user, type, amount range; export CSV firmado; hash verify button.

Imposible **borrar** eventos; soft-hide solo UI admin con audit del hide.
