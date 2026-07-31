# 20 — Performance Test Report

**Fecha:** 2026-07-30  
**Ambiente:** VPS PostgreSQL (`restbar_postgres`) — datos reales (no lab 1M)

## EXPLAIN (ANALYZE, BUFFERS) — branch de muestra

| SP | Execution Time | Notas |
|----|----------------|-------|
| `analytics.sp_sales_by_product` | **~1.4 ms** | Index `IX_orders_branch_status_opened` |
| `analytics.sp_inventory_health` | **~1.4 ms** | Seq scan products/movements (volumen bajo) |
| `analytics.sp_cash_summary` | **~0.16 ms** | Index `IX_cash_sessions_branch_opened` |

Script: `scripts/explain_analytics.sql` (todas las SPs ejecutadas &lt; 10 ms en volumen actual).

## Condición de certificación

- **Cumple objetivo &lt;2s** en dataset actual.
- Lab sintético **1M órdenes** sigue **DEFERRED** (no ejecutar en prod).
