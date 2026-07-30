# 04 — DATABASE DESIGN

## ALTER
- recipes: yield_percent decimal(8,4) default 100, target_food_cost_percent decimal(8,4), version int default 1
- recipe_lines: waste_percent decimal(8,4) default 0
- order_items: theoretical_unit_cost decimal(18,4) null, cost_snapshot_at timestamptz null

## CREATE
### food_cost_snapshots
id, company_id, branch_id, period_start, period_end, sales_total, theoretical_cogs, actual_cogs, variance_amount, variance_percent, waste_cost, food_cost_percent_theo, food_cost_percent_actual, generated_at, generated_by

### recipe_cost_histories
id, recipe_id, product_id, company_id, theoretical_cost, food_cost_percent, margin_amount, recorded_at, source

### waste_events
id, company_id, branch_id, product_id, station_id?, quantity, unit_cost, total_cost, reason_code, reason_notes, responsible_user_id, approved_by?, inventory_movement_id?, created_at

### variance_alerts
id, company_id, branch_id, alert_type, severity, message, period_start, period_end, variance_percent, is_resolved, created_at

### food_cost_audit_events
patrón hash chain (como Cash/Procurement)

## Índices
IX_food_cost_snapshots (branch_id, period_start)  
IX_waste_events (branch_id, created_at)  
IX_order_items (branch_id, created_at) parcial costo
