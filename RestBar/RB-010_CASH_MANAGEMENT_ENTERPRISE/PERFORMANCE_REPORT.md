# RB-010 — PERFORMANCE REPORT

**Fecha:** 2026-07-29  
**Metodología:** Revisión estática de consultas + índices definidos (sin load test automatizado)

---

## Índices creados

| Tabla | Índice | Propósito |
|-------|--------|-----------|
| cash_registers | UX(branch_id, code) | Lookup register |
| cash_sessions | IX(register_id, status) | Active session |
| cash_sessions | IX(branch_id, opened_at) | Dashboard |
| cash_movements | UX(session_id, sequence_number) | Ledger order |
| cash_movements | UX(idempotency_key) filtered | Payment idempotency |
| cash_movements | IX(session_id, created_at_utc) | Reports |
| cash_audit_events | IX(session_id, created_at_utc) | Audit trail |
| cash_z_reports | UX(session_id) | One Z per session |
| payments | IX(cash_session_id) | Reconciliation |

## Optimizaciones aplicadas

- `AsNoTracking()` en lecturas dashboard, reports, list movements
- Single query max for expected cash (cached on session entity)
- Hash chain computed in-process (no extra DB round-trip)
- Payment hook: 1–2 movement inserts max per payment (+ tip)

## Targets diseño vs medición

| Operación | Target P95 | Medido |
|-----------|------------|--------|
| Open session | 300ms | ⏳ UAT |
| Record movement | 150ms | ⏳ UAT |
| Payment hook overhead | +50ms | ⏳ UAT |
| Close + Z | 2s | ⏳ UAT |

**Conclusión:** Índices y patrones EF alineados al diseño. Benchmark runtime pendiente UAT.
