# 02 — Data Catalog

Evidence-based catalog of fields usable for native BI. Status: **AVAILABLE** | **PARTIAL** | **NO DISPONIBLE**.

## Dimensions

| Dimension | Source | Status | Notes |
|-----------|--------|--------|-------|
| Empresa | `companies` / `*.CompanyId` | AVAILABLE | |
| Sucursal | `branches` / `*.BranchId` | AVAILABLE | |
| Piso / Área | `areas` via `tables.area_id`, `stations.area_id` | AVAILABLE | |
| Mesa | `tables` / `orders.table_id` | AVAILABLE | |
| Mesero | `orders.user_id` → `users` | AVAILABLE | Role not enforced in order FK |
| Cajero | `payments.processed_by_user_id` | AVAILABLE | |
| Caja / sesión | `cash_sessions`, `payments.cash_session_id` | AVAILABLE | Feature flag gated |
| Estación | `stations`, `order_items.prepared_by_station_id` | AVAILABLE | |
| Producto | `products` | AVAILABLE | |
| Categoría | `categories` / `product_categories` | AVAILABLE | |
| Método de pago | `payments.method` (string) | PARTIAL | No `PaymentMethod` dimension entity |
| Cliente | `customers` / `orders.customer_id` | PARTIAL | Many orders may lack customer |
| Proveedor | `suppliers` | AVAILABLE | |
| Comprador | `purchase_orders.requested_by_user_id` | AVAILABLE | |

## Facts — Sales

| Fact | Fields | Status |
|------|--------|--------|
| Venta cerrada | `orders.total_amount`, `closed_at`, `status='Completed'` | AVAILABLE |
| Línea de venta | `order_items.quantity`, `unit_price`, `discount` | AVAILABLE |
| Descuento | `orders.discount_amount`, item discount | AVAILABLE |
| Pago | `payments.amount`, `paid_at`, `is_voided` | AVAILABLE |
| Propina | `payments.tip_amount` | AVAILABLE |
| Costo línea | `order_items.theoretical_unit_cost` OR `products.average_cost`/`cost` | PARTIAL | Dual costing paths |
| Hora/día/semana/mes | Derived from `closed_at` / `paid_at` | AVAILABLE | |

## Facts — Profitability / Food cost

| Fact | Source | Status |
|------|--------|--------|
| Utilidad bruta estimada | Revenue − COGS estimate | AVAILABLE (estimate) |
| Food cost % teórico/actual | `food_cost_snapshots` | AVAILABLE when snapshots generated |
| Costo por plato | recipes + recipe_lines + ingredient cost | AVAILABLE via Food Cost module |
| Costo por ingrediente | products + recipe_lines | AVAILABLE |
| Merma costo | `waste_events.total_cost` | AVAILABLE |
| Costo por proveedor | PO lines + receipts + price_history | PARTIAL | Needs aggregation |

## Facts — Inventory

| Fact | Source | Status |
|------|--------|--------|
| Stock actual | `products.stock` + `product_stock_assignments` | AVAILABLE |
| Stock histórico punto-en-tiempo | — | **NO DISPONIBLE** (reconstruct via kardex only) |
| Rotación / cobertura | Derived from movements + stock | PARTIAL |
| Merma / desperdicio | `waste_events`, movement type `Waste` | AVAILABLE |
| Consumo (venta) | movement type `Sale` | AVAILABLE |
| Vencimientos | `goods_receipt_lines.expiry_date` | PARTIAL | Only on receipt lines, not inventory lots |
| Productos inmovilizados | No last-movement date dimension | PARTIAL (derivable) |

## Facts — Purchasing

| Fact | Source | Status |
|------|--------|--------|
| PO amount/date/status | `purchase_orders` | AVAILABLE |
| Lead time | `order_date` → first `goods_receipts.received_at` | AVAILABLE |
| Variación precio | `price_history` | AVAILABLE |
| Scores proveedor | `supplier_scores` | AVAILABLE |

## Facts — Cash

| Fact | Source | Status |
|------|--------|--------|
| Apertura/cierre | `cash_sessions.opened_at/closed_at` | AVAILABLE |
| Ventas/reembolsos/tips/paid in-out | session totals + `cash_movements` | AVAILABLE |
| Diferencias | `cash_sessions.variance` | AVAILABLE |
| Z report | `cash_z_reports` | AVAILABLE |

## Facts — Kitchen / orders ops

| Fact | Source | Status |
|------|--------|--------|
| Tiempo orden | `opened_at`→`closed_at` | AVAILABLE |
| Tiempo prep | `order_items.sent_at`→`prepared_at` | AVAILABLE (was hardcoded 0; fixed RB-025) |
| Cancelaciones | status + cancellation logs | AVAILABLE |
| Ticket promedio | sales / completed orders | AVAILABLE |

## Facts — Customers

| Fact | Status |
|------|--------|
| Frecuencia / retención / favoritos | PARTIAL — joinable via `customer_id` when populated; no loyalty visit ledger beyond points |
