# 09 — CASH SESSION

---

# Definición

**CashSession** = ciclo de responsabilidad sobre un `CashRegister` desde apertura hasta cierre auditado.

**Shift** = turno laboral del empleado. Relación opcional `ShiftId` para correlacionar mesero/cajero con labor, **sin duplicar** lógica Shift.

---

# Ciclo de vida (resumen)

```
Prepared → Open → Operating ⇄ Suspended → Counting → Reconciling → Closed
                                                      ↘ Blocked (incident)
Closed → (Reopen authorized) → Open (new session linked)
Closed → Historical (immutable)
```

Detalle en doc 13.

---

# Apertura (Opening Wizard)

1. Seleccionar Register (solo activos branch)  
2. Verificar no hay otra sesión Open en register  
3. Opcional: Start/attach Shift  
4. Declarar fondo inicial (denominaciones o total)  
5. Supervisor witness (si policy)  
6. Confirm → Status=Open, Movement OpeningFloat  
7. SignalR: `CashSessionOpened`  

---

# Operación

- Todos los pagos efectivo POS auto-ligan sesión del cajero activo (o register seleccionado)  
- Paid-in/out manuales desde Cash Dashboard  
- Mid-shift drop: partial count + Movement DropToSafe (opcional v1.1)  

---

# Cierre

1. Validar no órdenes pendientes pago (warning, no block configurable)  
2. Suspender nuevos cobros efectivo (`Status=Counting`)  
3. Blind count UI  
4. Calcular Expected vs Counted  
5. Si |Variance| > threshold → CashApproval pending  
6. Manager/Supervisor approve o document reason  
7. Snapshot Z-report → `cash_z_reports`  
8. Status=Closed, ClosedAt=UTC  
9. Tips summary from TipAllocation  
10. SignalR + Command Center update  

---

# Campos calculados (snapshotted at close)

| Campo | Fuente |
|-------|--------|
| ExpectedCash | ReconciliationService |
| CountedCash | CashCount Closing |
| Variance | Counted - Expected |
| TotalSales | SUM payments completed |
| TotalRefunds | PaymentRefund |
| TotalTips | TipAllocation |
| TotalPaidIn/Out | Movements |

---

# Reapertura

Solo `Closed` sessions within 24h (config); creates new session with `ReopenedFromSessionId`; original remains Historical; requires manager + incident.

---

# Multi-session per day

Permitido: turno mañana/tarde/noche en mismo register = 3 sessions; Z consolidado día = report agregado.
