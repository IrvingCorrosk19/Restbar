# RB-020 — BUILD / PERF / REGRESSION / SECURITY / MT / TECHNICAL

## BUILD
`dotnet build` → 0 errors · `dotnet test` → 45/45 PASS

## PERFORMANCE
Índices: suppliers(company,code), PO(branch,status), GRN(company,number), price_history(product,date), inv_mov(goods_receipt_id).  
AsNoTracking en dashboard/reports. Receipt aplica costo por línea en misma TX lógica.

## REGRESSION
Payment/Cash/Order/KDS sin modificación funcional. Ad-hoc `CreatePurchase` permanece. Hook solo vía GoodsReceipt.

## SECURITY
Policies PurchasingAccess / CostingAccess activas en controllers. Dual approval PO ≥ $500. Blacklist hard-stop. Audit hash chain. Flag OFF = ModuleDisabled.

## MULTITENANT
Supplier Company-scoped. PR/PO/GRN Company+Branch. Queries por claims.

## TECHNICAL DECISIONS
1. No Warehouse — Station como ubicación recepción.  
2. Extender InventoryMovement (FK + UnitCost) — no duplicar ledger.  
3. Product.Cost = AverageCost (WAC) post-receipt.  
4. Patrón RB-010: partial context, feature flag, hash audit.  
5. Supplier JS routes reutilizadas (`GetSuppliers`, etc.).  
6. Quotes/Returns/3-way AP invoice = v1.1.
