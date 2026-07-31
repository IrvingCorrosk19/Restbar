# 04 — Data Quality Report

Validaciones recomendadas (no auto-corregir histórico):

| Anomalía | Detección | Acción |
|----------|-----------|--------|
| Pedidos sin BranchId/CompanyId | COUNT WHERE NULL | Corregir operación/seed |
| Líneas sin producto | order_items.product_id NULL | Bloquear en POS |
| Pagos void mezclados en analytics | filtrar is_voided=false | Ya en sp_sales_by_payment |
| Costos nulos | COGS coalesce a 0 | Marca margen como estimado |
| Timestamps prep inconsistentes | prepared_at < sent_at | Excluidos en kitchen SP |
| Food cost sin snapshots | snapshot_count=0 | UI limitation badge |
| Dualidad Cost vs AverageCost vs Theoretical | documentado en KPI catalog | Política única pendiente |

No se ejecuta silent fix de datos históricos.
