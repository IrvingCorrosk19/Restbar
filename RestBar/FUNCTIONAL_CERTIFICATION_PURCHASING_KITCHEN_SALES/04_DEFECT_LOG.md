# 04 — DEFECT LOG

**Fecha:** 2026-07-28  
**Fuente:** ejecución PKS + análisis de código + retest

| ID | Módulo | Severidad | Título | Estado | Evidencia |
|----|--------|-----------|--------|--------|-----------|
| DEF-PO-001 | Purchasing | **Critical** | Módulo Órdenes de Compra inexistente (sin entidades/API/UI) | **OPEN** | 12 endpoints 404; 29 escenarios BLOCKED |
| DEF-PO-002 | Purchasing | **Critical** | Proveedores no implementados (JS huérfano `supplier-management.js`) | **OPEN** | `/Supplier/*` 404 |
| DEF-PO-003 | Purchasing | High | `SupplierAnalysis` / PO reports son stubs en cero | **OPEN** | PO-SUB-03 |
| DEF-PO-004 | Purchasing | Medium | `CreatePurchase` no persiste costo unitario pese a UI | **OPEN** | `MovementDto` sin UnitCost |
| DEF-PO-005 | Purchasing | Medium | StockTransfer: `Rejected` en enum sin endpoint Reject | **OPEN** | Código StockTransferController |
| DEF-SALE-001 | Sales | **Critical** | Sin módulo de caja (apertura/arqueo/cierre) | **OPEN** | SALE-GAP-03 / SB-02 |
| DEF-SALE-002 | Sales | **Critical** | Sin precuenta ni factura fiscal post-pago | **OPEN** | SALE-GAP-04 |
| DEF-SALE-003 | Sales | High | Sin combos | **OPEN** | SALE-GAP-02 |
| DEF-SALE-004 | Sales | High | Sin Happy Hour automático | **OPEN** | SALE-GAP-01 |
| DEF-SALE-005 | Sales | Medium | Cortesía no tipificada | **OPEN** | SALE-GAP-05 |
| DEF-SALE-006 | Sales | Medium | Delivery sin UI operativa | **OPEN** | SALE-07 nota |
| DEF-KDS-001 | Kitchen | Medium | `MoveToTable` no re-enruta ítems a estaciones del nuevo piso | **OPEN** | Documentado + SALE-10 |
| DEF-KDS-002 | Kitchen | Low | `KitchenService` registrado en DI pero no usado por controllers | **OPEN** | Código muerto |
| DEF-SEED-001 | Setup/Security | High | `EnsureUserAsync` no reactivaba usuarios canónicos desactivados → login mesero fallaba | **FIXED** | SeedController + retest |
| DEF-ORD-001 | Sales/Orders | High | `SendToKitchen` con body null → NullReferenceException | **FIXED** | Guard en OrderController + build |
| DEF-UI-PAY-01 | Sales | **Critical** | Modal pago `onclick=processPayment()` sin args → error servidor en browser | **FIXED** | Index.cshtml + payments.js |
| DEF-UI-TAX-01 | Sales | High | taxRate decimal (0.07) dividido /100 en UI | **FIXED** | order-ui.js |
| DEF-UI-TBL-01 | Sales | Medium | Mesa P1-02 duplicada en grid POS | **OPEN** | Evidencia browser |

---

## Resumen

| Severidad | OPEN | FIXED |
|-----------|------|-------|
| Critical | 4 | 0 |
| High | 5 | 2 |
| Medium | 5 | 0 |
| Low | 1 | 0 |

**Regla de certificación:** presencia de Critical/High abiertos en flujos esenciales ⇒ **FAIL**.
