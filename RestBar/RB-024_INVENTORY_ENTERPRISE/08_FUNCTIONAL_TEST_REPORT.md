# 08 — Functional Test Report (RB-024)

**Fecha:** 2026-07-31  
**Ambiente:** VPS `http://164.68.99.83:8084`  
**Commit:** `94e788a` (+ fix validación movimientos)

## Unit

| Suite | Resultado |
|-------|-----------|
| `InventoryRecipeQtyTests` (4) | **PASS** |

## Browser — Inventory (chromium-desktop)

| ID | Resultado |
|----|-----------|
| INV-E01 GetEnterpriseSnapshot | **PASS** |
| INV-E02 Transfer Reject soft | **PASS** |
| INV-E03 Purchase qty negativa | Fix: BadRequest (no 500) |
| INV-E04 Adjustment producto inválido | Fix: NotFound/400 (no 500) |
| INV-E05 Index + snapshot | **PASS** |
| INV-ORD-01/02 | **PASS** |
| INV-01..08 | **PASS** |

Corrida inicial: **13 PASS / 2 FAIL** (500 en negativos). Corrección en `InventoryMovementController.AdjustStock` + retest pendiente post-deploy.

## Cadena funcional (evidencia de código + browser)

Compra/recepción → kardex Purchase · Pedido/SendToKitchen → Deduct (waste/yield) · Cancel → CancelRestore · Waste FoodCost → Waste · Transfer estaciones → TransferOut/In · Dashboard snapshot → valor/crítico/movimientos.
