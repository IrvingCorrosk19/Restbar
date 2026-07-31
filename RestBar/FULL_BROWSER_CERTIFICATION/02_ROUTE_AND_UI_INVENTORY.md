# 02 — ROUTE AND UI INVENTORY

**Fecha:** 2026-07-30 · **Commit:** `0ab2bd2`

## Default route
`{controller}/{action=Index}/{id?}` + attribute API routes.

## Primary navigation (`_Layout.cshtml`)
Home · Configuración (Company, Branch, Area, Table, Category, Station, Product) · Operaciones (CommandCenter, ExecutiveAnalytics, BiNative, Copilot, Order, Kitchen, CashSession, CashRegister, Procurement, Supplier, PurchaseOrder, FoodCost, Recipe) · Users · Profile · Logout.

## Not in nav (direct / Home cards)
Inventory, ProductStockAssignment, PaymentView, AdvancedSettings, AdvancedReports, Reports, UserAssignment, Audit, Seed, SuperAdmin, CashReport, StockTransfer, Email.

## API-only (no dedicated view folder)
`api/analytics`, `api/CashMovement`, `api/Payment`, `api/kitchen`, InventoryMovement, Person, Shift.

## Orphans / gaps
| Item | Note |
|------|------|
| `Views/Payment/Index.cshtml` | Huérfana — UI viva es PaymentView |
| `EmailController` | Return View sin `Views/Email` |
| Reports ExportPdf/Excel | Stub “en desarrollo” |

## SignalR
Hub `/orderHub` — stations, orders, tables, kitchen, cash dashboard.
