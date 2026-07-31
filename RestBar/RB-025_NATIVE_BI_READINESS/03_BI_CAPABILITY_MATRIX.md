# 03 — BI Capability Matrix

Estado: **LISTO** | **PARCIAL** | **NO DISPONIBLE**

| Indicador | Datos disponibles | Consulta posible | Falta información | Estado |
|-----------|-------------------|------------------|-------------------|--------|
| Ventas por hora | `orders.closed_at` | `sp_hourly_sales` / Reports | — | LISTO |
| Ventas por día/semana/mes/año | `closed_at` | date_trunc en SQL o app | — | LISTO |
| Ventas por sucursal | `BranchId` | `sp_branch_comparison` | — | LISTO |
| Ventas por empresa | `CompanyId` | filtro tenant | — | LISTO |
| Ventas por piso/área | `tables.area_id`→`areas` | join | UI nativa no dedicada | PARCIAL |
| Ventas por mesa | `table_id` | join | — | LISTO |
| Ventas por mesero | `orders.user_id` | `sp_waiter_performance` | — | LISTO |
| Ventas por cajero | `payments.processed_by_user_id` | join payments | no SP dedicado | PARCIAL |
| Ventas por caja/sesión | `cash_session_id` | join | flag cash | LISTO |
| Ventas por método pago | `payments.method` | group by method | sin catálogo maestro | PARCIAL |
| Ventas por producto | order_items | `sp_top_products` | — | LISTO |
| Ventas por categoría | product→category | AdvancedReports | — | LISTO |
| Utilidad bruta | revenue − cogs est. | `sp_profitability` | costo dual (Product.Cost vs AverageCost vs TheoreticalUnitCost) | PARCIAL |
| Margen % | derivado | `sp_profitability` | misma dualidad | PARCIAL |
| Food Cost % | snapshots | `sp_food_cost_summary` / FoodCostDashboard | requiere generación de snapshots | PARCIAL |
| Costo por plato | recipes | FoodCost module | — | LISTO |
| Costo por ingrediente | recipe_lines | FoodCost | — | LISTO |
| Costo por categoría | join | app | no SP | PARCIAL |
| Costo por proveedor | PO/receipts | `sp_supplier_analysis` | — | LISTO |
| Stock actual | products + PSA | `sp_inventory_health` | — | LISTO |
| Stock histórico | kardex only | reconstruct | tabla snapshot | NO DISPONIBLE |
| Rotación / cobertura | movements + stock | derive | no SP dedicado | PARCIAL |
| Merma / desperdicio | waste_events | `sp_top_waste` | — | LISTO |
| Consumo | Sale movements | filter type | — | LISTO |
| Productos críticos | min_stock vs stock | inventory health | — | LISTO |
| Productos inmovilizados | last movement age | derive | no SP | PARCIAL |
| Compras por proveedor/sucursal/tiempo | POs | `sp_purchase_analysis` | — | LISTO |
| Lead time | order→receipt | `avg_lead_days` | — | LISTO |
| Variación precio | price_history | Procurement services | no SP | PARCIAL |
| Caja aperturas/cierres/varianza | cash_sessions | `sp_cash_summary` | module flag | LISTO |
| Entradas/salidas caja | cash_movements | query | — | LISTO |
| Reembolsos | refunds + session totals | — | — | LISTO |
| Tiempo prep cocina | sent_at/prepared_at | `sp_station_performance` | timestamps deben poblarse en operación | LISTO |
| Retrasos cocina | vs target SLA | — | no SLA target table | NO DISPONIBLE |
| Cancelaciones | status | sales summary | — | LISTO |
| Ticket promedio | revenue/orders | SPs | — | LISTO |
| Frecuencia cliente | orders by customer | AdvancedReports | customer_id sparse | PARCIAL |
| Retención cliente | — | cohort logic | no cohort table | NO DISPONIBLE |
| Alertas ejecutivas | bi_alerts / Command Center | app | — | LISTO |
| Forecast | forecast_seeds | table exists | engine limitado | PARCIAL |

## Resumen cuantitativo

| Estado | Count (approx) |
|--------|----------------|
| LISTO | 28 |
| PARCIAL | 14 |
| NO DISPONIBLE | 4 |
