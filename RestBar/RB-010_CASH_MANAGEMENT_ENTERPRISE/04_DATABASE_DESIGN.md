# 04 — DATABASE DESIGN

**Motor:** PostgreSQL · EF Core · Multitenant CompanyId+BranchId **NOT NULL** en tablas nuevas

---

# Tablas nuevas (justificadas)

| Tabla | ¿Nueva? | Justificación |
|-------|---------|---------------|
| `cash_registers` | **Sí** | Config recurso; no existe equivalente |
| `cash_sessions` | **Sí** | Aggregate cierre; Shift ≠ dinero |
| `cash_movements` | **Sí** | Ledger inmutable; Payment no es ledger |
| `cash_counts` | **Sí** | Arqueo denominaciones |
| `cash_count_lines` | **Sí** | Normalización denominaciones |
| `cash_approvals` | **Sí** | Dual control workflow |
| `cash_incidents` | **Sí** | Incident management |
| `cash_audit_events` | **Sí** | Forense + hash chain (AuditLog genérico insuficiente) |
| `cash_z_reports` | **Sí** | Snapshot inmutable al cierre (JSON + totals) |

## NO crear (reutilizar)

| Evitar | Usar |
|--------|------|
| CashPayment | `payments` + `cash_movements` |
| CashShift | `shifts` + `cash_sessions.shift_id` |
| CashInvoice | `invoices` (precuenta fase posterior) |
| Duplicate tips table | `tip_allocations` |

---

# Alteraciones mínimas existentes

```sql
ALTER TABLE payments ADD COLUMN cash_session_id UUID NULL;
ALTER TABLE payments ADD CONSTRAINT FK_payments_cash_session 
  FOREIGN KEY (cash_session_id) REFERENCES cash_sessions(id);
CREATE INDEX IX_payments_cash_session ON payments(cash_session_id);

ALTER TABLE payment_refunds ADD COLUMN cash_session_id UUID NULL;
```

Nullable en migración v1 para no romper histórico; **nuevos** pagos efectivo requieren sesión (app rule).

---

# Esquema principal (DDL conceptual)

```sql
-- cash_registers
id UUID PK, company_id UUID NOT NULL, branch_id UUID NOT NULL,
code VARCHAR(20) NOT NULL, name VARCHAR(100),
register_type VARCHAR(30), default_opening_float NUMERIC(18,2),
requires_blind_close BOOLEAN DEFAULT false,
variance_threshold_amount NUMERIC(18,2),
station_id UUID NULL,
is_active BOOLEAN DEFAULT true,
UNIQUE(branch_id, code)

-- cash_sessions  
id UUID PK, company_id, branch_id, cash_register_id,
shift_id UUID NULL, session_number INT,
status VARCHAR(30) NOT NULL,
opened_at TIMESTAMPTZ, closed_at TIMESTAMPTZ,
opened_by_user_id, closed_by_user_id,
supervisor_user_id, manager_user_id,
opening_float_declared NUMERIC(18,2),
expected_cash, counted_cash, variance NUMERIC(18,2),
total_sales, total_refunds, total_tips, total_paid_in, total_paid_out,
blind_close_enabled BOOLEAN, close_notes TEXT,
reopened_from_session_id UUID NULL,
row_version BYTEA -- xmin/concurrency token

-- cash_movements (append-only)
id UUID PK, cash_session_id NOT NULL,
movement_type VARCHAR(40), direction VARCHAR(3),
amount NUMERIC(18,2) CHECK (amount >= 0),
payment_id, order_id, payment_refund_id UUID NULL,
related_movement_id UUID NULL,
reason_code VARCHAR(50), comments TEXT,
performed_by_user_id, authorized_by_user_id,
sequence_number INT NOT NULL,
previous_hash VARCHAR(64), record_hash VARCHAR(64),
idempotency_key VARCHAR(100) UNIQUE NULL,
source VARCHAR(20), created_at_utc TIMESTAMPTZ,
company_id, branch_id NOT NULL

-- cash_counts + cash_count_lines (standard)
-- cash_approvals, cash_incidents, cash_audit_events, cash_z_reports
```

---

# Índices (performance)

```sql
IX_cash_sessions_branch_status_opened (branch_id, status, opened_at)
IX_cash_sessions_register_active (cash_register_id, status) WHERE status IN ('Open','Operating')
IX_cash_movements_session_seq (cash_session_id, sequence_number)
IX_cash_movements_payment (payment_id) WHERE payment_id IS NOT NULL
IX_cash_movements_created (branch_id, created_at_utc)
IX_cash_registers_branch_active (branch_id, is_active)
```

---

# Integridad referencial

- `cash_sessions.cash_register_id` → ON DELETE RESTRICT  
- `cash_movements.cash_session_id` → ON DELETE RESTRICT (nunca cascade delete ledger)  
- Payments: ON DELETE SET NULL on cash_session (histórico)  

---

# Migración estrategia

1. **M1:** Tablas cash_* sin enforcement  
2. **M2:** UI apertura + link payments nuevos  
3. **M3:** Enforcement `RequireOpenSessionForCashPayments` (branch setting)  
4. **M4:** Backfill opcional sesión "legacy" por día  

---

# Escalabilidad

- Particionar `cash_movements` por `created_at_utc` mensual si >50M rows (año 3+)  
- `cash_z_reports` JSONB comprimido; cold storage  
- Read model materialized view `mv_cash_daily_branch` para Command Center  
