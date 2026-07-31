# 01 — Data Source Inventory

**Estado:** PASS · **Fecha:** 2026-07-30 · Evidencia: modelos EF + schema `analytics` + SPs RB-025

| Fuente | Entidad/Tabla | Campos relevantes | Granularidad | Frecuencia | Tenant | Sucursal | Calidad | Uso analítico |
|--------|---------------|-------------------|--------------|------------|--------|----------|---------|---------------|
| POS Orders | `orders` / `analytics.v_completed_orders` | total_amount, discount, status, closed_at, user_id, table_id, customer_id | Orden | Tiempo real | company_id | branch_id | Alta si Completed | Ventas, forecast input, ticket |
| Order lines | `order_items` / `v_order_lines` | qty, unit_price, discount, costs, sent_at, prepared_at, product_id | Línea | Tiempo real | company_id | branch_id | Media (costos coalesce) | Producto, FC, kitchen, menu eng |
| Payments | `payments` | amount, tip_amount, method, is_voided, processed_by | Pago | Tiempo real | vía order | vía order | Alta filtrando void | Métodos, tips, cashier |
| Refunds | `payment_refunds` | amount | Evento | Tiempo real | vía payment | — | Media | Reembolsos |
| Split | `split_payments` | amounts | Pago | Tiempo real | — | — | Media | Limitado en DI v1 |
| Cash sessions | `cash_sessions` | open/close, variance, totals | Sesión | Diario ops | company | branch | Alta módulo on | Cash risk |
| Cash movements | `cash_movements` | type, amount | Movimiento | Ops | company | branch | Alta | Anomalías determinísticas |
| Z reports | `cash_z_reports` | totals | Cierre | Diario | company | branch | Alta | Cierre día |
| Products | `products` | stock, cost, average_cost, price, track_inventory | SKU | Continuo | company | — | Media costos | Inventario, cobertura |
| Inv movements | `inventory_movements` | type, qty, unit_cost | Movimiento | Ops | company | branch | Media | Rotación, consumo |
| Stock assign | `product_stock_assignments` | stock by station | SKU×estación | Ops | — | — | Media | Ubicación débil |
| Transfers | `stock_transfers` | qty, status | Transfer | Ops | company | branches | Media | Pendientes |
| Recipes | `recipes` / `recipe_lines` | yield, waste%, qty | BOM | Config | company | branch? | Media | FC teórico |
| FC snapshots | `food_cost_snapshots` | theo/actual % | Snapshot | Job/manual | company | branch | Baja si no generado | FC real |
| Waste | `waste_events` | cost, reason | Evento | Ops | company | branch | Media | Merma |
| Suppliers | `suppliers` + scores | OTIF, scores | Maestro | Config | company | — | Media | Procurement |
| PO / GR | `purchase_orders`, `goods_receipts`, lines | totals, expiry_date | Doc | Ops | company | branch | Media | Compras, expiry parcial |
| Price history | `price_histories` | unit price | Evento | Ops | company | — | Media | Variación precios |
| Users / waiters | `users` | role, name | Maestro | Config | company | branch | Alta | Ventas por mesero |
| Shifts | `shifts` | started_at, ended_at | Turno | Ops | company | branch | Media | Workforce **sin costo** |
| Tips alloc | `tip_allocations` | amount, user_id | Split | Ops | — | — | Media | Tips por persona |
| Tables/Areas | `tables`, `areas` | status, area_id | Maestro | Ops | branch | branch | Alta | Turnover |
| Stations | `stations` | name | Maestro | Config | branch | branch | Alta | Kitchen |
| Customers | `customers` | basic | Maestro | Ops | company | — | Baja uso | RFM **DEFERRED** |
| Audit | `audit_logs` | actions | Evento | Continuo | — | — | Media | Seguridad |
| BI insights | `bi_insights` | type, action | Insight | Generado | company | branch | N/A | Recomendaciones |
| BI alerts | `bi_alerts` | code, severity | Alerta | Generado | company | branch | N/A | Riesgos |
| Forecast seeds | `forecast_seeds` | metric, value | Diario | Histórico | company | branch | Solo histórico | Baseline seed |
| Operating hours | `operating_hours` | DOW open/close | Semanal | Config | company | — | Alta | Capacidad; **no festivos** |
| Currencies | `currencies` | code, rate | Maestro | Config | company | — | Media | Default USD analytics |
| Tax rates | `tax_rates` | rate | Maestro | Config | company | — | Media | KPI tax **NotAvailable** consolidado |

## Fuentes faltantes (no inventar)

| Necesidad DI | Estado |
|--------------|--------|
| Calendario festivos | NOT APPLICABLE |
| Clima | NOT APPLICABLE |
| Nómina / costo laboral | NOT APPLICABLE |
| Cover count / guests | RequiresModelChange |
| Conteo físico inventario | NotAvailable |
| Expiry a nivel lote stock | Solo goods_receipt_lines |
| Presupuesto formal | No entidad |
