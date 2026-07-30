# 04 — DATABASE DESIGN

---

# Tablas nuevas

## suppliers
| Columna | Tipo | Notas |
|---------|------|-------|
| id | uuid PK | |
| company_id | uuid NOT NULL | MT |
| code | varchar(30) | unique per company |
| name | varchar(200) | |
| tax_id | varchar(50) | RUC/NIT |
| email, phone | varchar | |
| payment_terms_days | int | default 30 |
| lead_time_days | int | default 2 |
| status | varchar(20) | enum |
| is_preferred | bool | |
| score_overall | decimal(5,2) | cached |
| notes | varchar(1000) | |
| created_at/updated_at, created_by/updated_by | | ITrackable |

**UX:** `(company_id, code)` · **IX:** `(company_id, status)`

## supplier_contacts
id, supplier_id, name, role, email, phone, is_primary

## supplier_products
id, supplier_id, product_id, company_id, supplier_sku, pack_size, unit_of_measure,  
agreed_unit_price, currency_code, min_order_qty, is_active, lead_time_override_days  
**UX:** `(supplier_id, product_id)` WHERE is_active

## purchase_requests / purchase_request_lines
PR: company_id, branch_id, request_number, status, requested_by, approved_by, notes, dates  
Line: product_id, quantity, unit, preferred_supplier_id?, estimated_unit_cost, station_id?

## purchase_orders / purchase_order_lines
PO: company_id, branch_id, supplier_id, po_number, status, order_date, expected_delivery,  
subtotal, tax, total, currency, requested_by, approved_by, sent_at, closed_at,  
purchase_request_id?, row_version  
Line: product_id, supplier_product_id?, quantity_ordered, quantity_received,  
unit_price, line_total, unit_of_measure, station_id?, notes

**UX:** `(company_id, po_number)` · **IX:** `(branch_id, status)`, `(supplier_id, status)`

## goods_receipts / goods_receipt_lines
GR: company_id, branch_id, purchase_order_id, receipt_number, status, received_at,  
received_by, supervised_by, notes, temperature_ok?  
Line: purchase_order_line_id, product_id, qty_ordered, qty_received, qty_accepted,  
qty_rejected, disposition, unit_cost, lot_number?, expiry_date?, notes

## purchase_approvals
session-like: entity_type (PR/PO), entity_id, approval_type, status, amounts, users, timestamps

## supplier_scores
supplier_id, company_id, period_start/end, price_score, otif_score, quality_score,  
reliability_score, overall_score, computed_at

## price_history
id, company_id, product_id, supplier_id?, unit_cost, source (Receipt/Manual/Import),  
goods_receipt_id?, recorded_at — **append only**

## procurement_audit_events
patrón CashAuditEvent: company_id, branch_id, event_type, actor, before/after json,  
previous_hash, event_hash, ip, device, created_at_utc

---

# Alteraciones mínimas (aditivas)

## products
+ last_purchase_cost decimal(18,2)  
+ average_cost decimal(18,2)  
+ last_purchase_at timestamptz  
*(Cost existente se mantiene = AverageCost sync o configurable)*

## inventory_movements
+ goods_receipt_id uuid NULL FK  
+ purchase_order_id uuid NULL FK  
+ supplier_id uuid NULL FK  
+ unit_cost decimal(18,2) NULL  

---

# Constraints

- CHECK qty_accepted + qty_rejected <= qty_received + tolerance  
- PO total = sum lines (app-enforced + optional DB trigger later)  
- No DELETE físico en PriceHistory / Audit / completed PO (soft cancel)

---

# Rollback

Feature flag OFF + migration down (drop new tables, drop additive columns).  
Datos históricos de Product.Cost previos intactos.
