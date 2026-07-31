# 02 — Data Source Audit

See also RB-025_NATIVE_BI_READINESS/01-02.

| Métrica | Fuente | Campo | Confiabilidad | Historial | Estado |
|---------|--------|-------|---------------|-----------|--------|
| Ventas | orders | total_amount, closed_at, status | Alta si Completed | closed_at | DISPONIBLE |
| Líneas | order_items | qty, unit_price, discount | Alta | via order | DISPONIBLE |
| Descuentos | orders | discount_amount | Media (doble vía POS) | sí | DISPONIBLE CON LIMITACIONES |
| Propinas | payments | tip_amount | Alta | paid_at | DISPONIBLE |
| Impuestos | — | — | — | — | NO DISPONIBLE |
| Métodos pago | payments | method string | Media | paid_at | DISPONIBLE CON LIMITACIONES |
| Caja | cash_sessions | variance, totals | Alta | opened_at | DISPONIBLE |
| Reembolsos | cash_sessions / payment_refunds | totals | Media | sí | DISPONIBLE CON LIMITACIONES |
| Inventario stock | products.stock + PSA | stock | Alta actual | NO snapshot | DISPONIBLE / historial NO |
| Merma | waste_events | total_cost | Alta | created_at | DISPONIBLE |
| Food cost | food_cost_snapshots | FC% | Alta si hay snapshots | period | DISPONIBLE CON LIMITACIONES |
| Compras | purchase_orders | total, status | Alta | order_date | DISPONIBLE |
| Precios proveedor | price_history | unit_cost | Alta | recorded_at | DISPONIBLE |
| Prep cocina | order_items | sent_at, prepared_at | Media (nullable) | sí | DISPONIBLE CON LIMITACIONES |
| Comensales | — | — | — | — | REQUIERE CAMBIO DE MODELO |
| Stock reservado | — | — | — | — | NO DISPONIBLE |
| Bodega WMS | stations as location | — | — | — | REQUIERE CAMBIO DE MODELO |
| Conteo físico | — | — | — | — | NO DISPONIBLE |
