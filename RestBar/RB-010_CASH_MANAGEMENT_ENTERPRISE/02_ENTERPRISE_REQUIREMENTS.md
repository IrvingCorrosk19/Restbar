# 02 — ENTERPRISE REQUIREMENTS

**Formato:** REQ-ID · Prioridad · Fuente · Criterio aceptación

---

# Funcionales — Core

| ID | Requisito | Pri | Aceptación |
|----|-----------|-----|------------|
| FR-001 | Registrar `CashRegister` por Branch | P0 | CRUD manager; multitenant |
| FR-002 | Abrir `CashSession` con fondo inicial contado | P0 | No operar cobros efectivo sin sesión abierta (configurable) |
| FR-003 | Vincular cada `Payment` efectivo a `CashSessionId` | P0 | Backfill opcional; nuevos pagos obligatorio |
| FR-004 | Registrar paid-in / paid-out con motivo | P0 | Movimiento en ledger; auditoría |
| FR-005 | Calcular expected cash en tiempo real | P0 | Fórmula documentada; reproducible |
| FR-006 | Arqueo con denominaciones (CashCount) | P0 | Billetes/monedas; total contado |
| FR-007 | Cierre con diferencia documentada | P0 | Sobrante/faltante; motivo si ≠0 |
| FR-008 | Z-report por sesión y consolidado día | P0 | PDF/Excel export |
| FR-009 | Cierre parcial (mid-shift drop) | P1 | Nueva sesión o sub-session según diseño |
| FR-010 | Cambio de cajero con handoff | P1 | Extiende Shift handoff + CashSession transfer |
| FR-011 | Blind close mode | P1 | Cajero no ve expected hasta post-count |
| FR-012 | Reapertura autorizada manager | P1 | Audit + reason mandatory |
| FR-013 | Bloqueo pagos si sesión suspendida | P0 | Payment API valida |
| FR-014 | Propinas: acumular y reportar en cierre | P1 | TipAllocation link |
| FR-015 | Reembolso impacta expected cash | P0 | PaymentRefund → movement |
| FR-016 | Void payment impacta ledger | P0 | IsVoided handling |
| FR-017 | Pagos tarjeta/Yappy/ACH tracked, no en drawer | P0 | Método no suma a expected cash |
| FR-018 | Pagos mixtos desglosados | P0 | Ya existe SplitPayment; cash portion only |
| FR-019 | Command Center widgets caja | P1 | Snapshot API |
| FR-020 | Reportes rol cajero→CEO | P1 | Ver doc 16 |

---

# No funcionales

| ID | Requisito | Target |
|----|-----------|--------|
| NFR-001 | Multitenant isolation | 100% tests IDOR |
| NFR-002 | Cierre sesión P95 | < 2s cálculo expected |
| NFR-003 | Apertura wizard | < 60s UX |
| NFR-004 | Inmutabilidad movimientos | No UPDATE amount; solo reversal |
| NFR-005 | UTC timestamps | All entities |
| NFR-006 | Concurrencia | RowVersion en CashSession |
| NFR-007 | Escalabilidad | 1000 branches; ver doc 05 |
| NFR-008 | No romper ORDER cert | Regression 119/119 |
| NFR-009 | Audit retention | Configurable; min 7 años diseño |
| NFR-010 | Feature flag | `EnableCashModule` (Foundation) |

---

# Seguridad

| ID | Requisito |
|----|-----------|
| SEC-001 | Policy `CashAccess` mínimo |
| SEC-002 | Dual approval varianza > umbral |
| SEC-003 | Manager override logged |
| SEC-004 | Segregación: cajero no aprueba propios voids |
| SEC-005 | TenantScope en todas las APIs |
| SEC-006 | SignalR groups por branch/register |

---

# Integración (sin reimplementar)

| ID | Requisito |
|----|-----------|
| INT-001 | PaymentService hook post-create → CashMovement auto |
| INT-002 | PaymentController sin duplicar lógica cobro |
| INT-003 | ShiftController coexist; CashSession.ShiftId optional |
| INT-004 | AuditMiddleware + CashAuditEvent |
| INT-005 | NotificationService alertas varianza |

---

# Exclusiones explícitas v1

- Nueva tabla Payment paralela  
- Segundo Shift system  
- Caja sin Register  
- Edición silenciosa de montos históricos  
- Excel como export único  
